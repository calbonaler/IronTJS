using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IronTjs
{
	public partial class InputBox : Form
	{
		public InputBox()
		{
			InitializeComponent();
		}

		public string Description
		{
			get { return lblDescription.Text; }
			set { lblDescription.Text = value; }
		}

		public string InputText { get { return txtInput.Text; } }
	}
}
