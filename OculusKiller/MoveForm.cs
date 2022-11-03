using System.Windows.Forms;

namespace OculusKillerInstaller
{
    class MoveForm
    {
        public static bool drag = false;
        public static int mousex;
        public static int mousey;

        public static void MouseDown(object sender)
        {
            drag = true;
            sender = Form.ActiveForm;
            var move = (Form)sender;
            mousex = System.Windows.Forms.Cursor.Position.X - move.Left;
            mousey = System.Windows.Forms.Cursor.Position.Y - move.Top;
        }

        public static void MouseMove(object sender)
        {
            if (drag)
            {
                sender = Form.ActiveForm;
                var move = (Form)sender;
                move.Top = System.Windows.Forms.Cursor.Position.Y - mousey;
                move.Left = System.Windows.Forms.Cursor.Position.X - mousex;
            }
        }

        public static void MouseUp()
        {
            drag = false;
        }
    }
}
