using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using MVVMDynamic.Internal;

namespace MVVMDynamic
{
    public class TypeEmitter
    {
        private Dictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();
        private const string _assemblyName = "DynamicallyEmitted";
#if UNITY_EDITOR || !(UNITY_IOS || UNITY_WEBGL)
        private ILGeneratingUtility _generatingUtility;
#endif

        private static TypeEmitter _instance;
        private Type[] _allTypes;
        private List<Type> _viewModelInterfaces;

        public List<Type> ViewModelInterfaces
        {
            get { return _viewModelInterfaces; }
        }

        public static TypeEmitter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TypeEmitter();
                }
                return _instance;
            }
        }

        private TypeEmitter()
        {
            _allTypes = ReflectionExtensions.AllTypes;
            _viewModelInterfaces = FindViewModelInterfaces(_allTypes);

#if UNITY_EDITOR || !(UNITY_IOS || UNITY_WEBGL)
            _generatingUtility = new ILGeneratingUtility();
#endif
        }

        private List<Type> FindViewModelInterfaces(Type[] types)
        {
            List<Type> result = new List<Type>();
            foreach (Type type in types)
            {
                if (type.IsInterface && type != typeof (IViewModel) && typeof (IViewModel).IsAssignableFrom(type))
                {
                    result.Add(type);
                }
            }

            return result;
        }

        public T CreateViewModel<T>()
            where T : IViewModel
        {
            Type type = GetViewModelType<T>();
            T newInstance = (T) Activator.CreateInstance(type);
            return newInstance;
        }

        public static string GetDynamicallyGeneratedTypeName(Type interfaceType)
        {
            string typeName = "DG_" + interfaceType.Name.Remove(0, 1);
            return typeName;
        }

        private static string GetFullName(Type propertyType)
        {
            if (!propertyType.IsGenericType)
            {
                return propertyType.FullName;
            }

            string name = propertyType.FullName;
            name = name.Remove(name.LastIndexOf('`'));
            var Targs = propertyType.GetGenericArguments().Select(p => GetFullName(p)).ToArray();
            name += String.Format(
                "<{0}>",
                String.Join(",", Targs)
                );

            return name;
        }

        public Type GetViewModelType<T>() where T : IViewModel
        {
            Type interfaceType = typeof (T);
            return GetViewModelType(interfaceType);
        }

        private Type GetViewModelType(Type interfaceType)
        {
            Type result;
            if (!_typeMap.TryGetValue(interfaceType, out result))
            {
#if UNITY_EDITOR || !(UNITY_IOS || UNITY_WEBGL) //JIT
                result = _generatingUtility.EmitType(interfaceType);
#else //Baked sources
                string typeName = GetDynamicallyGeneratedTypeName(interfaceType);
                result = Type.GetType(typeName);
                if (result == null)
                {
                    throw new TypeLoadException(
                        String.Format(
                            "MVVMDynamic: Type {0} not found. Failed to create viewmodel for {1}. Please regenerate sources.",
                            typeName,
                            interfaceType.Name
                            ));
                }
#endif

                _typeMap[interfaceType] = result;
            }

            return result;
        }

        public void GenerateSources()
        {
#if UNITY_EDITOR
            foreach (Type interfaceType in _viewModelInterfaces)
            {
                GetViewModelType(interfaceType);
            }
            _generatingUtility.SaveSources();
#endif
        }

#if UNITY_EDITOR || !(UNITY_IOS || UNITY_WEBGL)
        private class ILGeneratingUtility
        {
            private ModuleBuilder _moduleBuilder;
            private AssemblyBuilder _assemblyBuilder;
            private const string _notifyMethodName = "NotifyPropertyChanged";
            private Type _baseType = typeof (ViewModelBase);
            private MethodInfo _notifyMethodInfo;
            private MethodInfo _objectEqualsMethodInfo;
            private Dictionary<Type, TypeSource> _sourceMap = new Dictionary<Type, TypeSource>();

            public ILGeneratingUtility()
            {
                AssemblyName assemblyName = new AssemblyName(_assemblyName);
                AppDomain appDomain = AppDomain.CurrentDomain;
                _assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
                _moduleBuilder = _assemblyBuilder.DefineDynamicModule(assemblyName.Name, _assemblyName + ".dll");
                _notifyMethodInfo = _baseType.GetMethod(_notifyMethodName);
                _objectEqualsMethodInfo = typeof (Object).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public);
            }

            private class PropertyNode
            {
                public TypeBuilder TypeBuilder;
                public PropertyInfo PropertyInfo;
                public FieldBuilder FieldBuilder;
                public PropertyBuilder PropertyBuilder;
                public string BackFieldName;
                public string Name;
                public Type PropertyType;
                public PropertySource Source;

                public PropertyNode()
                {
                    Source = new PropertySource();
                }
            }

            private class PropertySource
            {
                public string SetterBody;
                public string GetterBody;
                public string PropertyBody;
            }

            private class MethodNode
            {
                public TypeBuilder TypeBuilder;
                public MethodInfo MethodInfo;
                public string SourceBody;
                public ParameterInfo[] Args;
            }

            private class TypeNode
            {
                public List<PropertyNode> PropertyNodes;
                public List<MethodNode> MethodNodes;
                public TypeSource Source;
                public TypeBuilder TypeBuilder;
                public Type InterfaceType;
                public Type BaseType;

                public TypeNode()
                {
                    MethodNodes = new List<MethodNode>();
                    PropertyNodes = new List<PropertyNode>();
                    Source = new TypeSource();
                }
            }

            private class TypeSource
            {
                public string TypeBody;
                public string TypeName;
            }

            public Type EmitType(Type interfaceType)
            {
                string typeName = GetDynamicallyGeneratedTypeName(interfaceType);
                TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AnsiClass |
                                                TypeAttributes.BeforeFieldInit;
                TypeBuilder typeBuilder = _moduleBuilder.DefineType(
                    typeName,
                    typeAttributes,
                    _baseType);

                typeBuilder.AddInterfaceImplementation(interfaceType);
                typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

                TypeNode typeNode = new TypeNode
                {
                    TypeBuilder = typeBuilder,
                    InterfaceType = interfaceType,
                    BaseType = _baseType
                };

                typeNode.Source.TypeName = typeName;

                foreach (var prop in interfaceType.GetProperties())
                {
                    string name = prop.Name;
                    string backFieldName = "_" + name;
                    Type propertyType = prop.PropertyType;

                    PropertyNode node = new PropertyNode
                    {
                        TypeBuilder = typeBuilder,
                        PropertyInfo = prop,
                        Name = prop.Name,
                        BackFieldName = backFieldName,
                        PropertyType = propertyType
                    };

                    BuildField(node);
                    BuildProperty(node);

                    BuildGetter(node);
                    BuildSetter(node);

                    GeneratePropertyBody(node);

                    typeNode.PropertyNodes.Add(node);
                }

                foreach (var method in interfaceType.GetMethods())
                {
                    if (method.IsSpecialName) continue;
                    if (!method.IsPublic) continue;
                    if (method.ReturnType != typeof (void)) continue;

                    MethodNode methodNode = new MethodNode
                    {
                        TypeBuilder = typeBuilder,
                        MethodInfo = method
                    };

                    BuildMethod(methodNode);

                    typeNode.MethodNodes.Add(methodNode);
                }

                Type result = typeBuilder.CreateType();

                GenerateTypeBody(typeNode);
#if UNITY_EDITOR
                _sourceMap[interfaceType] = typeNode.Source;
#endif
                return result;
            }


            public void SaveAssembly()
            {
                _assemblyBuilder.Save(_assemblyName + ".dll");
            }

            [Conditional("UNITY_EDITOR")]
            public void SaveSources()
            {
                string dir = "Assets/Generated";
                Directory.CreateDirectory(dir);
                foreach (var pair in _sourceMap)
                {
                    FileWriteAllText(
                        dir + "/" + pair.Value.TypeName + ".cs",
                        pair.Value.TypeBody
                        );
                }
            }

            private void FileWriteAllText(string path, string text)
            {
#if !UNITY_WEBPLAYER
                File.WriteAllText(path, text);
#else
                Directory.CreateDirectory(Directory.GetParent(path).FullName);
                using (TextWriter tw = new StreamWriter(path))
                {
                    tw.Write(text);
                }
#endif
            }

            [Conditional("UNITY_EDITOR")]
            private void GenerateTypeBody(TypeNode typeNode)
            {
                var props = String.Join(Environment.NewLine,
                    typeNode.PropertyNodes.Select(p => p.Source.PropertyBody).ToArray());

                var methods = String.Join(Environment.NewLine, typeNode.MethodNodes.Select(p => p.SourceBody).ToArray());

                typeNode.Source.TypeBody = String.Format(
                    "#if !UNITY_EDITOR" + Environment.NewLine
                    + @"
    public class {0} : {1}, {2}
    {{
           {3}
           {4}
    }}"
                    + Environment.NewLine + "#endif",
                    typeNode.Source.TypeName,
                    GetFullName(typeNode.BaseType),
                    GetFullName(typeNode.InterfaceType),
                    props,
                    methods
                    );
            }

            [Conditional("UNITY_EDITOR")]
            private static void GeneratePropertyBody(PropertyNode node)
            {
                node.Source.PropertyBody = String.Format(@"
        private {0} {1};
        public {0} {2}
            {{
                get
                {{
                    {3}
                }}
                set
                {{
                    {4}
                }}
            }}",
                    GetFullName(node.PropertyType),
                    node.BackFieldName,
                    node.Name,
                    node.Source.GetterBody,
                    node.Source.SetterBody
                    );
            }

            [Conditional("UNITY_EDITOR")]
            private static void GenerateGetterBody(PropertyNode node)
            {
                node.Source.GetterBody = String.Format(
                    @"return this.{0};",
                    node.BackFieldName
                    );
            }

            [Conditional("UNITY_EDITOR")]
            private static void GenerateSetterBodyPlain(PropertyNode node)
            {
                node.Source.SetterBody = String.Format(
                    @"this.{0} = value;",
                    node.BackFieldName
                    );
            }

            [Conditional("UNITY_EDITOR")]
            private static void GenerateSetterBodyChecked(PropertyNode node)
            {
                node.Source.SetterBody = String.Format(@"
                    if (this.{0} != value)
			        {{
				        {1} oldValue = this.{0};
				        this.{0} = value;
				        base.{3}(""{2}"", oldValue, this.{0});
			        }}",
                    node.BackFieldName,
                    GetFullName(node.PropertyType),
                    node.Name,
                    _notifyMethodName
                    );
            }

            [Conditional("UNITY_EDITOR")]
            private void GenerateMethodBody(MethodNode node)
            {
                string methodArgsStr = String.Join(
                    ",",
                    node.Args.Select(p =>
                        String.Format("{0} {1}", GetFullName(p.ParameterType), p.Name)
                        ).ToArray());

                string notifyArgsStr = "";
                for (int i = 0; i < 2; i++)
                {
                    if (i > 0) notifyArgsStr += ",";
                    if (node.Args.Length > i)
                    {
                        notifyArgsStr += node.Args[i].Name;
                    }
                    else
                    {
                        notifyArgsStr += "null";
                    }
                }

                node.SourceBody = String.Format(@"
                    public void {0}({1})
	                {{
		                base.{3}(""{0}"", {2});
                    }}",
                    node.MethodInfo.Name,
                    methodArgsStr,
                    notifyArgsStr,
                    _notifyMethodName
                    );
            }

            private void BuildMethod(MethodNode node)
            {
                var args = node.MethodInfo.GetParameters();
                node.Args = args;
                Type[] paramTypes = args.Select(p => p.ParameterType).ToArray();

                MethodBuilder methodBuilder = node.TypeBuilder.DefineMethod(
                    node.MethodInfo.Name,
                    MethodAttributes.Public
                    | MethodAttributes.HideBySig | MethodAttributes.Final
                    | MethodAttributes.Virtual | MethodAttributes.NewSlot
                    ,
                    null,
                    paramTypes);

                ILGenerator metILG = methodBuilder.GetILGenerator();
                metILG.Emit(OpCodes.Ldarg_0);
                metILG.Emit(OpCodes.Ldstr, node.MethodInfo.Name);

                //Arg1
                if (args.Length > 0)
                {
                    metILG.Emit(OpCodes.Ldarg_1);
                    Type paramType = paramTypes[0];
                    if (paramType.IsValueType)
                    {
                        metILG.Emit(OpCodes.Box, paramType);
                    }
                }
                else
                {
                    metILG.Emit(OpCodes.Ldnull);
                }

                //Arg2
                if (args.Length > 1)
                {
                    metILG.Emit(OpCodes.Ldarg_2);
                    Type paramType = paramTypes[1];
                    if (paramType.IsValueType)
                    {
                        metILG.Emit(OpCodes.Box, paramType);
                    }
                }
                else
                {
                    metILG.Emit(OpCodes.Ldnull);
                }

                metILG.EmitCall(OpCodes.Call, _notifyMethodInfo, null);
                metILG.Emit(OpCodes.Ret);

                GenerateMethodBody(node);
            }

            private void BuildProperty(PropertyNode node)
            {
                PropertyAttributes propertyAttributes = PropertyAttributes.None;
                node.PropertyBuilder = node.TypeBuilder.DefineProperty(
                    node.Name,
                    propertyAttributes,
                    node.PropertyType,
                    null);
            }

            private void BuildGetter(PropertyNode node)
            {
                MethodBuilder getMethodBuilder = node.TypeBuilder.DefineMethod(
                    "get_" + node.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName
                    | MethodAttributes.HideBySig | MethodAttributes.Final
                    | MethodAttributes.Virtual | MethodAttributes.NewSlot,
                    node.PropertyType,
                    Type.EmptyTypes);

                ILGenerator getILG = getMethodBuilder.GetILGenerator();

                getILG.Emit(OpCodes.Ldarg_0);
                getILG.Emit(OpCodes.Ldfld, node.FieldBuilder);
                getILG.Emit(OpCodes.Ret);

                node.PropertyBuilder.SetGetMethod(getMethodBuilder);

                GenerateGetterBody(node);
            }

            private void BuildSetter(PropertyNode node)
            {
                Type type = node.PropertyType;

                OptInAttribute optin = node.PropertyInfo.DeclaringType.GetCustomAttribute<OptInAttribute>();
                RiseChangedEventAttribute rceAttr = node.PropertyInfo.GetCustomAttribute<RiseChangedEventAttribute>();

                MethodBuilder setMethodBuilder = node.TypeBuilder.DefineMethod(
                    "set_" + node.Name,
                    MethodAttributes.Public | MethodAttributes.SpecialName
                    | MethodAttributes.HideBySig | MethodAttributes.Final
                    | MethodAttributes.Virtual | MethodAttributes.NewSlot
                    ,
                    null,
                    new[] {type});

                ILGenerator setILG = setMethodBuilder.GetILGenerator();

                if (optin == null ||
                    (rceAttr != null && rceAttr.Rise))
                {
                    setILG.DeclareLocal(type);
                    Label onReturn = setILG.DefineLabel();

                    setILG.Emit(OpCodes.Ldarg_0);
                    setILG.Emit(OpCodes.Ldfld, node.FieldBuilder);
                    setILG.Emit(OpCodes.Ldarg_1);
                    if (type.IsPrimitive || type.IsEnum)
                    {
                        setILG.Emit(OpCodes.Beq, onReturn);
                    }
                    else
                    {
                        MethodInfo eqop = type.GetMethod("op_Inequality", BindingFlags.Static | BindingFlags.Public);
                        if (eqop != null)
                        {
                            setILG.EmitCall(OpCodes.Call, eqop, null);
                            setILG.Emit(OpCodes.Brfalse, onReturn);
                        }
                        else
                        {
                            setILG.EmitCall(OpCodes.Call, _objectEqualsMethodInfo, null);
                            setILG.Emit(OpCodes.Brtrue, onReturn);
                        }
                    }

                    setILG.Emit(OpCodes.Ldarg_0);
                    setILG.Emit(OpCodes.Ldfld, node.FieldBuilder);
                    setILG.Emit(OpCodes.Stloc_0);

                    setILG.Emit(OpCodes.Ldarg_0);
                    setILG.Emit(OpCodes.Ldarg_1);
                    setILG.Emit(OpCodes.Stfld, node.FieldBuilder);

                    setILG.Emit(OpCodes.Ldarg_0);
                    setILG.Emit(OpCodes.Ldstr, node.Name);

                    setILG.Emit(OpCodes.Ldloc_0);
                    if (type.IsValueType || type.IsEnum)
                    {
                        setILG.Emit(OpCodes.Box, type);
                    }

                    setILG.Emit(OpCodes.Ldarg_0);
                    setILG.Emit(OpCodes.Ldfld, node.FieldBuilder);

                    if (type.IsValueType || type.IsEnum)
                    {
                        setILG.Emit(OpCodes.Box, type);
                    }

                    setILG.EmitCall(OpCodes.Call, _notifyMethodInfo, null);

                    setILG.MarkLabel(onReturn);
                    setILG.Emit(OpCodes.Ret);

                    GenerateSetterBodyChecked(node);
                }
                else
                {
                    setILG.Emit(OpCodes.Ldarg_0);
                    setILG.Emit(OpCodes.Ldarg_1);
                    setILG.Emit(OpCodes.Stfld, node.FieldBuilder);
                    setILG.Emit(OpCodes.Ret);

                    GenerateSetterBodyPlain(node);
                }

                node.PropertyBuilder.SetSetMethod(setMethodBuilder);
            }

            private void BuildField(PropertyNode node)
            {
                node.FieldBuilder = node.TypeBuilder.DefineField(
                    node.BackFieldName,
                    node.PropertyType,
                    FieldAttributes.Private);
            }
        }
#endif
    }
}