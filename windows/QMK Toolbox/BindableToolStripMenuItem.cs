using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMK_Toolbox
{
    public class BindableToolStripMenuItem : ToolStripMenuItem, IBindableComponent
    {
        private BindingContext _bindingContext;
        private ControlBindingsCollection _dataBindings;

        [Browsable(false)]
        public BindingContext BindingContext
        {
            get
            {
                if (_bindingContext == null)
                {
                    _bindingContext = new BindingContext();
                }
                return _bindingContext;
            }
            set
            {
                _bindingContext = value;
            }
        }

        [Category("Data")]
        [ParenthesizePropertyName(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ControlBindingsCollection DataBindings
        {
            get
            {
                if (_dataBindings == null)
                {
                    _dataBindings = new ControlBindingsCollection(this);
                }
                return _dataBindings;
            }
        }
    }
}
