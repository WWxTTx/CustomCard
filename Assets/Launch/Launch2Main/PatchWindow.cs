using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameFramework.PatchEventDefine;
using static GameFramework.UserEventDefine;
namespace GameFramework
{
    public class PatchWindow : MonoBehaviour
    {
        /// <summary>
        /// 对话框封装类
        /// </summary>
        private class MessageBox
        {
            private GameObject _cloneObject;
            private Text _content;
            private Button _btnOK;
            private System.Action _clickOK;

            public bool ActiveSelf
            {
                get
                {
                    return _cloneObject.activeSelf;
                }
            }

            public void Create(GameObject cloneObject)
            {
                _cloneObject = cloneObject;
                _content = cloneObject.transform.Find("txt_content").GetComponent<Text>();
                _btnOK = cloneObject.transform.Find("btn_ok").GetComponent<Button>();
                _btnOK.onClick.AddListener(OnClickYes);
            }
            public void Show(string content, System.Action clickOK)
            {
                _content.text = content;
                _clickOK = clickOK;
                _cloneObject.SetActive(true);
                _cloneObject.transform.SetAsLastSibling();
            }
            public void Hide()
            {
                _content.text = string.Empty;
                _clickOK = null;
                _cloneObject.SetActive(false);
            }
            private void OnClickYes()
            {
                _clickOK?.Invoke();
                Hide();
            }
        }


        void Update()
        {
            // 检测返回键输入
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ShowMessageBox($"确定退出游戏吗？", () =>
                {
                    Application.Quit();
                });
            }
        }

        private readonly List<MessageBox> _msgBoxList = new List<MessageBox>();

        // UGUI相关
        private GameObject _messageBoxObj;
        private Slider _slider;
        private Text _tips;

        void Awake()
        {
            _slider = transform.Find("UIWindow/Slider").GetComponent<Slider>();
            _tips = transform.Find("UIWindow/Slider/txt_tips").GetComponent<Text>();
            _tips.text = "Initializing the game world !";
            _messageBoxObj = transform.Find("UIWindow/MessgeBox").gameObject;
            _messageBoxObj.SetActive(false);

            EventManager.AddListener<InitializeFailed>(OnHandleEventMessage, this);
            EventManager.AddListener<PatchStepsChange>(OnHandleEventMessage, this);
            EventManager.AddListener<FoundUpdateFiles>(OnHandleEventMessage, this);
            EventManager.AddListener<DownloadUpdate>(OnHandleEventMessage, this);
            EventManager.AddListener<PackageVersionRequestFailed>(OnHandleEventMessage, this);
            EventManager.AddListener<PackageManifestUpdateFailed>(OnHandleEventMessage, this);
            EventManager.AddListener<WebFileDownloadFailed>(OnHandleEventMessage, this);
        }
        void OnDestroy()
        {
            EventManager.RemoveOwnerListeners(this);
        }

        private void OnHandleEventMessage(InitializeFailed message)
        {
            System.Action callback = () =>
            {
                EventManager.PublishNow(new UserTryInitialize
                {
                });
            };
            ShowMessageBox($"Failed to initialize package !", callback);
        }
        private void OnHandleEventMessage(PatchStepsChange msg)
        {
            _tips.text = msg.Tips;
            Debug.Log(msg.Tips);
        }
        private void OnHandleEventMessage(FoundUpdateFiles msg)
        {
            System.Action callback = () =>
            {
                EventManager.PublishNow(new UserBeginDownloadWebFiles
                {
                });
            };
            float sizeMB = msg.TotalSizeBytes / 1048576f;
            sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
            string totalSizeMB = sizeMB.ToString("f1");
            ShowMessageBox($"Found update patch files, Total count {msg.TotalCount} Total szie {totalSizeMB}MB", callback);
        }
        private void OnHandleEventMessage(DownloadUpdate msg)
        {
            _slider.value = (float)msg.CurrentDownloadCount / msg.TotalDownloadCount;
            string currentSizeMB = (msg.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
            string totalSizeMB = (msg.TotalDownloadSizeBytes / 1048576f).ToString("f1");
            _tips.text = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount} {currentSizeMB}MB/{totalSizeMB}MB";
        }
        private void OnHandleEventMessage(PackageVersionRequestFailed msg)
        {
            System.Action callback = () =>
            {
                EventManager.PublishNow(new UserTryRequestPackageVersion
                {
                });
            };
            ShowMessageBox($"Failed to request package version, please check the network status.", callback);
        }
        private void OnHandleEventMessage(PackageManifestUpdateFailed msg)
        {
            System.Action callback = () =>
            {
                EventManager.PublishNow(new UserTryUpdatePackageManifest
                {
                });
            };
            ShowMessageBox($"Failed to request package version, please check the network status.", callback);
        }
        private void OnHandleEventMessage(WebFileDownloadFailed msg)
        {
            System.Action callback = () =>
            {
                EventManager.PublishNow(new UserTryDownloadWebFiles
                {
                });
            };
            ShowMessageBox($"Failed to download file : {msg.FileName}", callback);
        }
        /// <summary>
        /// 显示对话框
        /// </summary>
        private void ShowMessageBox(string content, System.Action ok)
        {
            // 尝试获取一个可用的对话框
            MessageBox msgBox = null;
            for (int i = 0; i < _msgBoxList.Count; i++)
            {
                var item = _msgBoxList[i];
                if (item.ActiveSelf == false)
                {
                    msgBox = item;
                    break;
                }
            }

            // 如果没有可用的对话框，则创建一个新的对话框
            if (msgBox == null)
            {
                msgBox = new MessageBox();
                var cloneObject = GameObject.Instantiate(_messageBoxObj, _messageBoxObj.transform.parent);
                msgBox.Create(cloneObject);
                _msgBoxList.Add(msgBox);
            }

            // 显示对话框
            msgBox.Show(content, ok);
        }
    }
}