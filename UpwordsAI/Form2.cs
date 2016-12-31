using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpwordsAI
{
    public partial class Form2 : Form
    {
        public bool goodclose = false;
        public Form2()
        {
            InitializeComponent();
        }

        public void Form2_Load(object sender, EventArgs e)
        { }

        private void button1_Click(object sender, EventArgs e)
        {
            if(TextBox1.Text.Length>0)
            {
                if (TextBox1.Text[0] >= 'A' && TextBox1.Text[0] <= 'Z')
                {
                    goodclose = true;
                    Close();
                }
                else
                    System.Media.SystemSounds.Exclamation.Play();
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox1.Text = TextBox1.Text.ToUpper();
            if (TextBox1.Text.Length>1)
            {
                TextBox1.Text = TextBox1.Text[1].ToString();
                TextBox1.Select(TextBox1.Text.Length, TextBox1.Text.Length );
            }
        }
    }
}
