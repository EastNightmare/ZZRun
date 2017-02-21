using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common;
using Assets.Scripts.Core.Models;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Core.Managers
{
    public class ScenesManager : SingletonMonoBehaviour<ScenesManager>
    {
        public int sceneIdx = 0;
        public GameObject girl;
        public GameObject boy;
        public GameObject love;
        public GameObject pauseMenu, pauseButton, failMenu, mainMenu, continuteMenu;
        public GameObject bkg;
        public GameObject[] stageIn;
        public AudioClip loveClip;
        public AudioClip failClip, loveHitClip, jumpThingClip;
        public GameObject yanhua;
        public AudioSource vioceAc;
        public GameObject quest;
        public GameObject jumpLost;
        public Button btnJump;
        public Slider slider;
        public List<SceneConfig> sceneConfig;
        private Dictionary<GameObject, float> m_Stacks = new Dictionary<GameObject, float>();
        private List<GameObject> m_ViewGOs = new List<GameObject>();
        private List<GameObject> m_Platforms = new List<GameObject>();
        private List<Tweener> m_Twners = new List<Tweener>();
        public float percent;

        public SceneConfig curSceneConfig
        {
            get { return sceneConfig[sceneIdx]; }
        }

        public void Pause()
        {
            pauseButton.SetActive(false);
            pauseMenu.SetActive(true);
            m_Twners.ForEach(t => t.timeScale = 0.0f);
            Time.timeScale = 0.0f;
        }

        public void StopAllTwners()
        {
            m_Twners.ForEach(t => t.timeScale = 0.0f);
        }

        public void Fail()
        {
            pauseButton.SetActive(false);
            failMenu.SetActive(true);
            m_Twners.ForEach(t => t.timeScale = 0.0f);
            Time.timeScale = 1.0f;
        }

        public void Play()
        {
            pauseButton.SetActive(true);
            pauseMenu.SetActive(false);
            failMenu.SetActive(false);
            m_Twners.ForEach(t => t.timeScale = 1.0f);
            Time.timeScale = 1.0f;
        }

        public void GameStart(int idx)
        {
            stageIn[idx].SetActive(true);
            pauseButton.SetActive(true);
            sceneIdx = idx;
            ResetGame();
            m_Platforms = CreatePlatforms();
            m_Stacks = CreateStacks(m_Platforms);
            m_ViewGOs = CreateViews();
            PlayBgm();

            var firstPlatform = m_Platforms[0].transform;
            var lastPlatform = m_Platforms[m_Platforms.Count - 1].transform;

            if (!girl.activeInHierarchy)
            {
                girl = Object.Instantiate<GameObject>(girl);
            }

            var girlScript = girl.GetComponent<Girl>();
            girlScript.btnJump = btnJump;

            var pos = new Vector3(curSceneConfig.characterStartX, firstPlatform.position.y, girl.transform.position.z);
            girl.transform.position = pos;

            if (sceneIdx == sceneConfig.Count - 1)
            {
                boy = Object.Instantiate<GameObject>(boy, lastPlatform);
                boy.transform.localPosition = new Vector3(1.9f, 0.0f, 0.0f);
            }
            else
            {
                var loveGO = Object.Instantiate<GameObject>(love, lastPlatform);
                loveGO.transform.localPosition = Vector3.zero;
            }
        }

        public void End()
        {
            m_Twners.ForEach(t => t.Kill());
            btnJump.gameObject.SetActive(false);
            girl.GetComponent<Girl>().enabled = false;
            girl.GetComponent<Rigidbody2D>().simulated = false;
            girl.transform.parent = m_Platforms[m_Platforms.Count - 1].transform;
            girl.transform.localPosition = new Vector3(girl.transform.localPosition.x, 0.0f, 0.0f);
            pauseButton.SetActive(false);
            slider.gameObject.SetActive(false);
            var signal = girl.transform.GetChild(3).gameObject;
            signal.SetActive(true);
            GetComponent<AudioSource>().DOFade(0.0f, 2.0f);
            m_Platforms[m_Platforms.Count - 1].transform.DOLocalMove(new Vector3(0.0f, -3.34f, 0.0f), 3.0f).SetDelay(1.5f).OnStart(
                () =>
                {
                    GetComponent<AudioSource>().clip = loveClip;
                    GetComponent<AudioSource>().Play();
                    GetComponent<AudioSource>().DOFade(1.0f, 2.0f);
                    signal.SetActive(false);
                }).OnComplete(() =>
                {
                    quest.SetActive(true);
                });
        }

        public void WalkClose()
        {
            girl.transform.DOLocalMove(new Vector3(-0.6f, 0.0f, 0.0f), 2.0f);
            boy.transform.DOLocalMove(new Vector3(0.6f, 0.0f, 0.0f), 2.0f).OnComplete(MakeLove);
        }

        public void MakeLove()
        {
            var loveGO = Object.Instantiate<GameObject>(love);
            loveGO.transform.position = new Vector3(0f, -6f, 0f);
            loveGO.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().DOFade(0.0f, 2.0f).SetEase(Ease.Linear);
            loveGO.transform.DOScale(Vector3.one * 10f, 2f).OnComplete(() =>
            {
                yanhua.SetActive(true);
                DOTweenUtils.Delay(() =>
                {
                    bkg.SetActive(true);
                }, 3.5f);
            });
        }

        public void Restart()
        {
            GameStart(sceneIdx);
            Play();
        }

        public void Continute()
        {
            var girlScript = girl.GetComponent<Girl>();
            var pos = new Vector3(curSceneConfig.characterStartX, 1.0f, girl.transform.position.z);
            girl.transform.position = pos;

            var thePercent = m_Twners[0].Elapsed() / curSceneConfig.time;
            m_Twners.ForEach(t =>
            {
                var time = 0.0f;
                if (thePercent >= 0.0f && thePercent < 0.25f)
                {
                    time = 0.0f;
                }
                else if (thePercent >= 0.25f && thePercent < 0.5f)
                {
                    time = curSceneConfig.time * 0.25f;
                }
                else if (thePercent >= 0.5f && thePercent < 0.75f)
                {
                    time = curSceneConfig.time * 0.5f;
                }
                else if (thePercent >= 0.75f && thePercent < 1.0f)
                {
                    time = curSceneConfig.time * 0.75f;
                }
                t.Goto(time, true);
            });
            Play();
            girlScript.Reborn();
        }

        public void SetTimeScale(float t)
        {
            Time.timeScale = t;
        }

        public void LoopScene(int idx)
        {
            sceneIdx = idx;
            ResetGame();
            m_Platforms = CreatePlatforms(() =>
            {
                if (++sceneIdx >= sceneConfig.Count)
                {
                    sceneIdx = 0;
                }
                LoopScene(sceneIdx);
            });
            m_Stacks = CreateStacks(m_Platforms);
            m_ViewGOs = CreateViews();
            PlayBgm();
        }

        public void ResetGame()
        {
            m_Platforms.ForEach(Object.Destroy);
            m_Platforms.Clear();
            girl.GetComponent<Girl>().jumpThings.Clear();
            m_Stacks.Keys.ToList().ForEach(Object.Destroy);
            m_Stacks.Clear();

            m_ViewGOs.ForEach(Object.Destroy);
            m_ViewGOs.Clear();
        }

        private void Start()
        {
            LoopScene(sceneIdx);
            mainMenu.SetActive(true);
            //slider.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (m_Twners != null && m_Twners.Count > 0)
            {
                slider.value = m_Twners[0].Elapsed() / curSceneConfig.time;
            }
        }

        private void PlayBgm()
        {
            var audioSource = gameObject.GetComponent<AudioSource>();
            audioSource = audioSource ?? gameObject.AddComponent<AudioSource>();
            audioSource.clip = curSceneConfig.bgm;
            audioSource.Play();
            audioSource.loop = true;
        }

        private List<GameObject> CreateViews()
        {
            var grounds = new List<GameObject>();
            var bkg = Object.Instantiate<GameObject>(curSceneConfig.bkg, curSceneConfig.bkgStartPos, Quaternion.identity, transform);
            var bkgTwner = bkg.transform.DOMove(curSceneConfig.bkgEndPos, curSceneConfig.time);
            m_Twners.Add(bkgTwner);
            var frg = Object.Instantiate<GameObject>(curSceneConfig.frg, curSceneConfig.frgStartPos, Quaternion.identity, transform);

            var frgTwner = frg.transform.DOMove(curSceneConfig.frgEndPos, curSceneConfig.time);
            m_Twners.Add(frgTwner);
            if (curSceneConfig.mdg)
            {
                var mdg = Object.Instantiate<GameObject>(curSceneConfig.mdg, curSceneConfig.mdgStartPos, Quaternion.identity, transform);
                var mdgTwner = mdg.transform.DOMove(curSceneConfig.mdgEndPos, curSceneConfig.time);
                m_Twners.Add(mdgTwner);
                grounds.Add(mdg);
            }
            grounds.Add(bkg);
            grounds.Add(frg);
            return grounds;
        }

        private Dictionary<GameObject, float> CreateStacks(List<GameObject> platforms)
        {
            var stacksDic = new Dictionary<GameObject, float>();
            var midPlatform = platforms.Select(p => p.transform.GetChild(0)).ToList();
            var stackNum = Mathf.RoundToInt(Random.Range(curSceneConfig.stackCountRange.x, curSceneConfig.stackCountRange.y));
            var lengthList = new List<int>();
            for (int i = 0; i < midPlatform.Count; i++)
            {
                var scale = Mathf.RoundToInt(midPlatform[i].transform.localScale.x);
                if (curSceneConfig.stackCountCurve.keys.Length != 0)
                {
                    scale = Mathf.RoundToInt(curSceneConfig.stackCountCurve.Evaluate((float)i / midPlatform.Count)) * 10;
                }
                for (int j = 0; j < scale; j++)
                {
                    lengthList.Add(i);
                }
            }
            for (int i = 0; i < stackNum; i++)
            {
                if (midPlatform.Count == 0) break;
                var indexOfIdx = Random.Range(0, lengthList.Count - 1);
                if (lengthList.Count == 0) break;
                var index = lengthList[indexOfIdx];

                var platform = midPlatform[index];
                var stacks = new GameObject("Stacks");
                stacks.transform.parent = platform.parent;
                var num = Mathf.RoundToInt(Random.Range(curSceneConfig.oneStackNumRange.x, curSceneConfig.oneStackNumRange.y));

                var startScale = Random.Range(curSceneConfig.stackScaleRange.x, curSceneConfig.stackScaleRange.y - 0.5f);
                var endScale = Random.Range(startScale, curSceneConfig.stackScaleRange.y);
                endScale = (endScale - startScale) < 0.5f ? curSceneConfig.stackScaleRange.y : endScale;
                var scaleOffset = (endScale - startScale) / num;
                if (curSceneConfig.stackScaleRange.x == curSceneConfig.stackScaleRange.y)
                {
                    startScale = curSceneConfig.stackScaleRange.x;
                    scaleOffset = 0.0f;
                }
                var startX = 0f;
                for (int j = 0; j < num; j++)
                {
                    var stack = Object.Instantiate<GameObject>(curSceneConfig.stack, Vector3.zero, Quaternion.identity, stacks.transform);
                    stack.transform.localScale = (startScale + j * scaleOffset) * Vector3.one;
                    stack.transform.localPosition = new Vector3(startX + curSceneConfig.stackWidthHeight.x * stack.transform.localScale.x / 2, curSceneConfig.stackWidthHeight.y / 2.0f * stack.transform.localScale.x, 0.0f);
                    startX += stack.transform.localScale.x * curSceneConfig.stackWidthHeight.x;
                }

                var offsetX = Random.Range(-platform.localScale.x / 2.0f * curSceneConfig.width + curSceneConfig.safeRange, platform.localScale.x / 2.0f * curSceneConfig.width - curSceneConfig.safeRange - startX);
                if (platform.localScale.x < 4.0f)
                {
                    offsetX = -startX / 2.0f;
                }
                stacks.transform.localPosition = new Vector3(offsetX, 0.0f, 0.0f);
                stacks.transform.localEulerAngles = Vector3.zero;

                var isDestroy = stacksDic.Keys.Any(s =>
                {
                    if (s.transform.parent == stacks.transform.parent)
                    {
                        if (s.transform.position.x - stacks.transform.position.x > 0)
                        {
                            return Vector3.Distance(s.transform.position, stacks.transform.position) - curSceneConfig.stackSafeDistance < startX;
                        }
                        else
                        {
                            return Vector3.Distance(s.transform.position, stacks.transform.position) - curSceneConfig.stackSafeDistance < stacksDic[s];
                        }
                    }
                    return false;
                });
                if (isDestroy)
                {
                    Object.Destroy(stacks);
                    stacks = null;
                }
                if (stacks != null)
                {
                    stacksDic.Add(stacks, startX);
                    lengthList.RemoveAt(indexOfIdx);
                }
            }
            return stacksDic;
        }

        private List<GameObject> CreatePlatforms(Action finishFunc = null)
        {
            m_Twners.Clear();
            var platforms = new List<GameObject>();
            var count = Mathf.RoundToInt(Random.Range(curSceneConfig.platformCountRange.x, curSceneConfig.platformCountRange.y));
            for (var i = 0; i < count; i++)
            {
                var x = i / (float)count;
                var length = Mathf.RoundToInt(Random.Range(curSceneConfig.widthRange.x, curSceneConfig.widthRange.y));
                if (curSceneConfig.widthCurve.keys.Length != 0)
                {
                    length = Mathf.RoundToInt(curSceneConfig.widthCurve.Evaluate(x) * length);
                }
                var height = Random.Range(curSceneConfig.heightRange.x, curSceneConfig.heightRange.y);
                if (curSceneConfig.heightCurve.keys.Length != 0)
                {
                    height = curSceneConfig.heightCurve.Evaluate(x) * height;
                }
                var go = CreateSinglePlatform(length, height);
                var preGo = i == 0 ? gameObject : platforms[i - 1];
                var preMid = preGo.transform.GetChild(0);
                var preGoScale = preMid != null ? preMid.localScale.x + 2.0f : 1.0f;
                var prePosX = i == 0 ? curSceneConfig.startX : preGo.transform.localPosition.x;
                var offsetX = Random.Range(curSceneConfig.offsetXRange.x, curSceneConfig.offsetXRange.y);
                if (curSceneConfig.offsetXCurve.keys.Length != 0)
                {
                    offsetX = Mathf.RoundToInt(curSceneConfig.offsetXCurve.Evaluate(x) * offsetX);
                }
                var offsetY = Random.Range(curSceneConfig.offsetYRange.x, curSceneConfig.offsetYRange.y);
                if (curSceneConfig.offsetYCurve.keys.Length != 0)
                {
                    offsetY = Mathf.RoundToInt(curSceneConfig.offsetYCurve.Evaluate(x) * offsetY);
                }
                var pos =
                    new Vector3(
                        prePosX + (preGoScale - 2 + length - 2) / 2 * curSceneConfig.width + 2 * curSceneConfig.sideWidth +
                        offsetX, offsetY, 0.0f);
                pos = i == 0 ? new Vector3(0.0f, -1.0f, 0.0f) : pos;
                go.transform.localPosition = pos;
                platforms.Add(go);
                var ran = Random.Range(0.0f, 1.0f);
                if (curSceneConfig.flipCurve.keys.Length != 0)
                {
                    curSceneConfig.flipPercent = curSceneConfig.flipCurve.Evaluate(x);
                }

                var platformOffsetY = Random.Range(curSceneConfig.platformOffsetY.x,
                       curSceneConfig.platformOffsetY.y);
                var platformOffsetX = Random.Range(curSceneConfig.platformOffsetX.x,
                       curSceneConfig.platformOffsetX.y);
                if (ran <= curSceneConfig.flipPercent)
                {
                    var flipGo = Object.Instantiate(go, go.transform);
                    flipGo.transform.localEulerAngles = new Vector3(flipGo.transform.localEulerAngles.x,
                        flipGo.transform.localEulerAngles.y, 180.0f);
                    flipGo.GetComponentsInChildren<Transform>().ToList().ForEach(t =>
                    {
                        t.gameObject.tag = "Stack";
                    });

                    if (curSceneConfig.platformOffsetCurve.keys.Length != 0)
                    {
                        platformOffsetY = Mathf.RoundToInt(curSceneConfig.platformOffsetCurve.Evaluate(x) * platformOffsetY);
                    }
                    flipGo.transform.localPosition += new Vector3(platformOffsetX, platformOffsetY, 0.0f);
                    if (flipGo.transform.position.y > Camera.main.orthographicSize)
                    {
                        flipGo.SetActive(false);
                    }
                }

                if (curSceneConfig.jumpIdxs.Contains(i))
                {
                    var jumpThing = Object.Instantiate<GameObject>(curSceneConfig.jumpThing, go.transform);
                    jumpThing.transform.localPosition = new Vector3(-go.transform.GetChild(0).localScale.x / 2 * curSceneConfig.width - curSceneConfig.sideWidth - 3.0f, platformOffsetY / 2, 0.0f);
                }
            }
            var firstWidth = (platforms[0].transform.GetChild(0).localScale.x * curSceneConfig.width +
                              2 * curSceneConfig.sideWidth) / 2;
            var lastWidth = (platforms[platforms.Count - 1].transform.GetChild(0).localScale.x * curSceneConfig.width +
                              2 * curSceneConfig.sideWidth) / 2;
            var allWidth = (platforms[platforms.Count - 1].transform.position.x - platforms[0].transform.position.x) +
                           firstWidth + lastWidth;
            curSceneConfig.time = allWidth / curSceneConfig.speed;
            platforms.ForEach(p =>
            {
                var endPos = p.transform.position.x - allWidth;
                var twn = p.transform.DOMoveX(endPos, curSceneConfig.time)
                    .SetEase(Ease.Linear)
                    .OnComplete(
                        () =>
                        {
                            if (curSceneConfig.isAutoDestroy)
                            {
                                Object.Destroy(p);
                            }
                            if (finishFunc != null && platforms.IndexOf(p) == platforms.Count - 1)
                            {
                                finishFunc();
                            }
                        });
                m_Twners.Add(twn);
            });
            return platforms;
        }

        private GameObject CreateSinglePlatform(int length, float height)
        {
            var platform = new GameObject("Platform");
            var midScale = length - 2;
            var mid = Object.Instantiate<GameObject>(curSceneConfig.midPlatform, new Vector3(0, -height * curSceneConfig.height / 2, 0), Quaternion.identity, platform.transform);
            mid.transform.localScale = new Vector3(mid.transform.localScale.x * midScale, mid.transform.localScale.y * height, mid.transform.localScale.z);
            var mtrl = mid.GetComponent<SpriteRenderer>().material;
            mtrl.SetFloat("_XTile", (float)midScale);
            mtrl.SetFloat("_YTile", height);
            var left = Object.Instantiate<GameObject>(curSceneConfig.sidePlatform, new Vector3((midScale / 2.0f) * -curSceneConfig.width - curSceneConfig.sideWidth / 2, -height * curSceneConfig.height / 2, 0.0f), Quaternion.identity, platform.transform);
            left.transform.localScale = new Vector3(left.transform.localScale.x, left.transform.localScale.y * height, left.transform.localScale.z);
            left.GetComponent<SpriteRenderer>().material.SetFloat("_YTile", height);
            var right = Object.Instantiate<GameObject>(curSceneConfig.sidePlatform, new Vector3((midScale / 2.0f) * curSceneConfig.width + curSceneConfig.sideWidth / 2, -height * curSceneConfig.height / 2, 0.0f), Quaternion.identity, platform.transform);
            right.transform.localScale = new Vector3(right.transform.localScale.x, right.transform.localScale.y * height, right.transform.localScale.z);
            right.GetComponent<SpriteRenderer>().material.SetFloat("_YTile", height);
            right.transform.localEulerAngles = new Vector3(right.transform.localEulerAngles.x, 180f, right.transform.localEulerAngles.z);
            platform.transform.parent = transform;

            return platform;
        }

        [Serializable]
        public class SceneConfig
        {
            public AudioClip bgm;
            public GameObject midPlatform;
            public GameObject sidePlatform;
            public GameObject bkg;
            public Vector3 bkgStartPos, bkgEndPos;
            public GameObject frg;
            public Vector3 frgStartPos, frgEndPos;
            public GameObject mdg;
            public Vector3 mdgStartPos, mdgEndPos;
            public Vector2 offsetXRange;
            public AnimationCurve offsetXCurve;
            public Vector2 offsetYRange;
            public AnimationCurve offsetYCurve;
            public Vector2 widthRange;
            public AnimationCurve widthCurve;
            public Vector2 heightRange;
            public AnimationCurve heightCurve;
            public GameObject stack;
            public Vector2 stackCountRange;
            public AnimationCurve stackCountCurve;
            public Vector2 platformCountRange;
            public float width = 0.39f;
            public float sideWidth = 0.46f;
            public float height = 2.69f;
            public Vector2 stackWidthHeight = new Vector2(1.29f, 1.51f);
            public Vector2 oneStackNumRange = new Vector2(1, 5);
            public AnimationCurve oneStackNumCurve;
            public Vector2 stackScaleRange = new Vector2(0.5f, 2f);
            public float stackSafeDistance = 2.0f;
            public AnimationCurve stackScaleCurve;
            public Vector2 platformOffsetY;
            public AnimationCurve platformOffsetCurve;
            public Vector2 platformOffsetX;
            public float speed;
            public float flipPercent;
            public AnimationCurve flipCurve;

            public List<int> jumpIdxs;
            public GameObject jumpThing;
            public float time { get; set; }
            public float startX = -5.4f;
            public float safeRange = 6.0f;
            public bool isAutoDestroy = false;
            public float characterStartX = -4.5f;
        }
    }
}