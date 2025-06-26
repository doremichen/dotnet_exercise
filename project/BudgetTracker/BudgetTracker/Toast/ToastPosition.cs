using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BudgetTracker.Toast
{
    public abstract class ToastPosition
    {

        public static readonly ToastPosition TopLeft = new TopLeftPosition();
        public static readonly ToastPosition TopRight = new TopRightPosition();
        public static readonly ToastPosition BottomLeft = new BottomLeftPosition();
        public static readonly ToastPosition BottomRight = new BottomRightPosition();
        public static readonly ToastPosition Center = new CenterPosition();

        // abtract getposition method
        public abstract Point GetPosition(double toastWidth, double toastHeight);

        private class TopLeftPosition : ToastPosition 
        { 
        
            public override Point GetPosition(double toastWidth, double toastHeight)
            {
                var wa = SystemParameters.WorkArea;
                return new Point(wa.Left + 10, wa.Top + 10);
            }
        }
        private class TopRightPosition : ToastPosition 
        { 
        
            public override Point GetPosition(double toastWidth, double toastHeight)
            {
                var wa = SystemParameters.WorkArea;
                return new Point(wa.Right - toastWidth - 10, wa.Top + 10);
            }
        }
        private class BottomLeftPosition : ToastPosition 
        { 
            
            public override Point GetPosition(double toastWidth, double toastHeight)
            {
                var wa = SystemParameters.WorkArea;
                return new Point(wa.Left + 10, wa.Bottom - toastHeight - 10);
            }
        }
        private class BottomRightPosition : ToastPosition 
        { 
        
            public override Point GetPosition(double toastWidth, double toastHeight)
            {
                var wa = SystemParameters.WorkArea;
                return new Point(wa.Right - toastWidth - 10, wa.Bottom - toastHeight - 10);
            }

        }
        private class CenterPosition : ToastPosition 
        { 
        
            public override Point GetPosition(double toastWidth, double toastHeight)
            {
                var wa = SystemParameters.WorkArea;
                return new Point((wa.Width - toastWidth) / 2 + wa.Left, (wa.Height - toastHeight) / 2 + wa.Top);
            }


        }
    }
}
