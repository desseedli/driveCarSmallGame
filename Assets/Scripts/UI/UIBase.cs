using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using UnityEngine.U2D;

namespace UIFramework
{

    public class UIBase : MonoBehaviour
    {
        [SerializeField]
        private Button m_backBtn;

        public bool IsInit { get; private set; }

        public UILayer Layer { get; set; }

        public UIInfo Info { get; private set; }

        public bool IsShowing { get { return m_status == UIStatus.Showing; } }

        private UIStatus m_status;

        public delegate void BackBtnDelegate(UIInfo ui);

        private event BackBtnDelegate m_CloseSelfEvent;

        private List<string> m_spriteAltaNames = new List<string>();
        private List<SpriteAtlas> m_spriteAtlas = new List<SpriteAtlas>();


        //关闭此UI(其他UI不关闭)的事件 
        public event BackBtnDelegate CloseSelfEvent
        {
            add
            {
                m_CloseSelfEvent += value;
            }
            remove
            {
                m_CloseSelfEvent -= value;
            }
        }

        /// <summary>
        /// 基于3D模型  UI有两个摄像机
        /// 1.UICamera 层级在3D模型之上的。
        /// 2.UIBGCamera 层级在3D模型之下的(一般都是背景)
        /// </summary>
        public static string[] Camera = new string[]
         {
           "UICamera",
           "UIBGCamera",
         };
        [ValueDropdown("Camera")]
        public string m_Camera = Camera[0];

        private Canvas canvas;
        public Canvas UICanvas
        {
            get
            {
                if (canvas == null)
                {
                    canvas = this.GetComponent<Canvas>();
                }
                return canvas;
            }
        }

        private GraphicRaycaster m_raycaster;
        public GraphicRaycaster Raycaster
        {
            get
            {
                if (m_raycaster == null)
                {
                    m_raycaster = this.GetComponent<GraphicRaycaster>();
                }
                return m_raycaster;
            }
        }

        public void SetSortOrder(int order)
        {
            UICanvas.sortingOrder = order;
        }

        public void InitUI(UIInfo info)
        {
            SetCamera();
            this.Info = info;
            UICanvas.planeDistance = 0;
            if (m_backBtn != null)
            {
                m_backBtn.onClick.AddListener(() =>
                {
                    OnClickCloseBtn();
                });
            }
            IsInit = true;
        }

        private void SetCamera()
        {
            if (string.IsNullOrEmpty(m_Camera))
            {
                Debug.LogError("请设置Camera摄像机类型！！！！1");
                return;
            }

            if(UICanvas.renderMode == RenderMode.WorldSpace)
            {
                UICanvas.worldCamera = GameObject.Find(m_Camera).GetComponent<Camera>();
            }
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        public IEnumerator ShowUI()
        {
            yield return BeforeShow();
            yield return InnerShow();
        }

        /// <summary>
        /// 用于处理打开一个ui以后的事件，如关闭其他所有ui
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadCompleteUI()
        {
            yield return AfterShow();
        }

        private IEnumerator InnerShow()
        {
            m_status = UIStatus.Showing;
            gameObject.CustomSetActive(true);
            Raycaster.enabled = true;

            Show();

            yield break;
        }

        /// <summary>
        /// 从隐藏状态重新显示UI
        /// </summary>
        public void ResumeUI()
        {
            gameObject.CustomSetActive(true);
            Raycaster.enabled = true;
            Resume();
        }


        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void HideUI()
        {
            m_status = UIStatus.Hide;
            Raycaster.enabled = false;
            gameObject.CustomSetActive(false);
            Hide();
        }

        /// <summary>
        /// 回收UI
        /// </summary>
        public void DeSpawnUI()
        {
            m_status = UIStatus.Despawn;
            m_CloseSelfEvent = null;
            Raycaster.enabled = false;
            DeSpawn();
            PoolManager.instance.DeSpawn<GameObject>(AssetType.Prefab, this.gameObject);
        }



        /// <summary>
        /// 点击关闭按钮
        /// </summary>
        public void OnClickCloseBtn()
        {
            ClickClose();
            if (m_CloseSelfEvent != null)
            {
                m_CloseSelfEvent(Info);
            }
        }


        #region 用于子类的继承
        protected virtual IEnumerator BeforeShow()
        {
            yield return LoadAtlasAsset();
            yield break;
        }

        protected virtual void Show()
        {

        }

        public virtual void PreAddAtlasAsset()
        {

        }

        public virtual void AddAtlasAsset(string atlasPath)
        {
            if(!m_spriteAltaNames.Contains(atlasPath))
            {
                m_spriteAltaNames.Add(atlasPath);
            }  
        }

        protected void SetImageSprite(Image img,string sprName)
        {
            Sprite resSpr = null;
            for(int i = 0; i < m_spriteAtlas.Count;++i)
            {
/*                int sprCount = m_spriteAtlas[i].spriteCount;
                Debug.Log(sprCount);*/
                Sprite tempSpr = m_spriteAtlas[i].GetSprite(sprName);
                if(tempSpr != null)
                {
                    resSpr = tempSpr;
                    break;
                }
            }
            img.sprite = resSpr;
        }

        private IEnumerator LoadAtlasAsset()
        {
            AsyncOperationHandle<IList<SpriteAtlas>> atlasHandle = Addressables.LoadAssetsAsync<SpriteAtlas>(m_spriteAltaNames, null, Addressables.MergeMode.Union);
            yield return atlasHandle;
            if (atlasHandle.Status == AsyncOperationStatus.Succeeded)
            {
                IList<SpriteAtlas> loadedAtlases = atlasHandle.Result;
                foreach (SpriteAtlas atlas in loadedAtlases)
                {
                    m_spriteAtlas.Add(atlas);
                }
            }
            else
            {
                Debug.LogError("Failed to load Atlases with labels.");
            }
        }

        protected virtual IEnumerator AfterShow()
        {
            yield break;
        }


        protected virtual void ClickClose()
        {

        }

        /// <summary>
        /// 重新显示
        /// </summary>
        protected virtual void Resume()
        {

        }

        /// <summary>
        /// 隐藏
        /// </summary>
        protected virtual void Hide()
        {

        }

        /// <summary>
        /// 回收
        /// </summary>
        protected virtual void DeSpawn()
        {

        }

        /// <summary>
        /// 摧毁
        /// </summary>
        protected virtual void Destroy()
        {

        }
        #endregion

    }

}