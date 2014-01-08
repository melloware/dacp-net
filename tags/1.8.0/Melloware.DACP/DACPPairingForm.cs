/*
   Melloware DACP.net - http://melloware.com

   Copyright (C) 2010 Melloware, http://melloware.com

   The Initial Developer of the Original Code is Emil A. Lefkof III.
   Copyright (C) 2010 Melloware Inc
   All Rights Reserved.
*/

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Melloware.DACP {
    /// <summary>
    /// Description of DACPPairingForm.
    /// </summary>
    public partial class DACPPairingForm : Form {
        public DACPPairingForm() {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
        }

        public string GetPassCode() {
            return this.txtPassCode.Text;
        }
    }
}
