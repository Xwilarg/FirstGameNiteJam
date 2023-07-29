using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FirstGameNiteJam.Menu
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField]
        private Button[] _buttons;

        [SerializeField]
        private GameObject _helpWindow, _creditsWindow;

        private Color _baseColor;
        private int _index;

        private bool _isSubWindowOn;

        private void Awake()
        {
            _baseColor = _buttons[0].colors.normalColor;
            Select();
        }

        public void StartGame()
        {
            SceneManager.LoadScene("Main");
        }

        public void RegisterSubWindow()
        {
            _isSubWindowOn = true;
        }

        public void Quit()
        {
            Application.Quit();
        }

        private void Unselect()
        {
            var cc = _buttons[_index].colors;
            cc.normalColor = _baseColor;
            _buttons[_index].colors = cc;
        }

        private void Select()
        {
            var cc = _buttons[_index].colors;
            cc.normalColor = Color.green;
            _buttons[_index].colors = cc;
        }

        public void OnMessage(string _, string msg)
        {
            var data = msg.Split(";");
            if (data.Length == 2 && data[1] == "1")
            {
                switch (data[0])
                {
                    case "{up}":
                        Unselect();
                        if (_index == 0) _index = _buttons.Length - 1;
                        else _index--;
                        Select();
                        break;

                    case "{down}":
                        Unselect();
                        if (_index == _buttons.Length - 1) _index = 0;
                        else _index++;
                        Select();
                        break;

                    case "{action}":
                        if (_isSubWindowOn)
                        {
                            _isSubWindowOn = false;
                            _creditsWindow.SetActive(false);
                            _helpWindow.SetActive(false);
                        }
                        else
                        {
                            _buttons[_index].onClick.Invoke();
                        }
                        break;
                }
            }
        }
    }
}
