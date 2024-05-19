using GTool.Input;
using GTool.Interface;
using GTool.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GEditor.Hub
{
    internal class NewProjectDialog
    {
        public bool Show = false;

        private string _projectPath = "";
        private string _projectName = "";

        public NewProjectDialog()
        {
            _projectPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _projectName = "New Project";
        }

        private bool _isHoldingCreate;
        private bool _isHoldingCancel;

        public void Update(Vector2 half)
        {
            if (!Show)
                return;

            Vector4 createRect = new Vector4(-half.X + 530.0f, -half.Y + 160.0f, half.X - 240.0f, -half.Y + 190.0f);
            bool isHoveringCreate = HitUtility.IsWithinRect(GHub.Instance.Input.Mouse - half, createRect);

            Vector4 cancelRect = new Vector4(-half.X + 390.0f, -half.Y + 160.0f, half.X - 380.0f, -half.Y + 190.0f);
            bool isHoveringCancel = HitUtility.IsWithinRect(GHub.Instance.Input.Mouse - half, cancelRect);

            if (_isHoldingCreate || (isHoveringCreate && GHub.Instance.Input.IsMouseDown(InputManager.MouseButton.Left)))
                _isHoldingCreate = GHub.Instance.Input.IsMouseDown(InputManager.MouseButton.Left) && !_isHoldingCancel && Show;
            else if (_isHoldingCancel || (isHoveringCancel && GHub.Instance.Input.IsMouseDown(InputManager.MouseButton.Left)))
                _isHoldingCancel = GHub.Instance.Input.IsMouseDown(InputManager.MouseButton.Left) && !_isHoldingCreate && Show;

            Gui.RectAB(new Vector4(-half.X, -half.Y, half.X, half.Y), 0x50000000);
            Gui.RectAB(new Vector4(-half.X + 230.0f, -half.Y + 150.0f, half.X - 230.0f, half.Y - 150.0f), 0xff202020);
            Gui.RectAB(new Vector4(-half.X + 230.0f, half.Y - 176.0f, half.X - 230.0f, half.Y - 150.0f), 0xff151515);
            Gui.RectAB(new Vector4(-half.X + 240.0f, half.Y - 250.0f, half.X - 240.0f, half.Y - 220.0f), 0xff101010);
            Gui.RectAB(new Vector4(-half.X + 245.0f, half.Y - 245.0f, half.X - 245.0f, half.Y - 225.0f), 0xff151515);
            Gui.RectAB(new Vector4(-half.X + 240.0f, half.Y - 331.0f, half.X - 240.0f, half.Y - 305.0f), 0xff101010);
            Gui.RectAB(new Vector4(-half.X + 245.0f, half.Y - 330.0f, half.X - 245.0f, half.Y - 310.0f), 0xff151515);
            Gui.RectAB(createRect, GHub.DecideColorIncrease(0xff333333, 0x00202020, isHoveringCreate, _isHoldingCreate));
            Gui.RectAB(cancelRect, GHub.DecideColorIncrease(0xff333333, 0x00202020, isHoveringCancel, _isHoldingCancel));

            Gui.Text("New project", new Vector2(-half.X + 235.0f, half.Y - 172.0f), 24, 0xffffffff);
            Gui.Text("Project name:", new Vector2(-half.X + 240.0f, half.Y - 210.0f), 20, 0xffffffff);
            if (_projectPath == string.Empty)
                Gui.Text("Target project path here..", new Vector2(-half.X + 250.0f, half.Y - 240.0f), 14, 0xff606060, 3.0f);
            else
                Gui.Text(_projectPath, new Vector2(-half.X + 250.0f, half.Y - 240.0f), 14, 0xffffffff);
            Gui.Text("Project name:", new Vector2(-half.X + 240.0f, half.Y - 295.0f), 20, 0xffffffff);
            if (_projectName == string.Empty)
                Gui.Text("Target project path here..", new Vector2(-half.X + 250.0f, half.Y - 325.0f), 14, 0xff606060, 3.0f);
            else
                Gui.Text(_projectName, new Vector2(-half.X + 250.0f, half.Y - 325.0f), 14, 0xffffffff);
            Gui.Text("Create", new Vector2(-half.X + 550.0f, -half.Y + 166.0f), 25, 0xffffffff);
            Gui.Text("Cancel", new Vector2(-half.X + 410.0f, -half.Y + 166.0f), 25, 0xffffffff);

            Gui.Text("ERROR ERROR ERROR", new Vector2(-half.X + 250.0f, half.Y - 268.5f), 14, 0xff0000ff);
            Gui.Text("ERROR ERROR ERROR", new Vector2(-half.X + 250.0f, half.Y - 353.5f), 14, 0xff0000ff);
        }
    }
}
