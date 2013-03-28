using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BLToolkit.Data;

namespace DbTableBuilder
{
    public partial class Form1 : Form
    {
        private TableManager manager = new TableManager();

        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.Filter = "Dll文件(*.dll)|*.dll";
            dialog.ShowDialog();
            //MessageBox.Show(dialog.FileName);
            manager.LoadAssembly(dialog.FileName);
            BindNamespaces();
            BindTypeNames(comboBox1.SelectedValue.ToString());
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            int count = checkedListBox1.Items.Count;
            if (count == 0) return;
            while (count-- > 0)
            {
                checkedListBox1.SetItemChecked(count, checkBox1.Checked);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindTypeNames(comboBox1.SelectedValue.ToString());
        }

        private void BindNamespaces()
        {
            comboBox1.DataSource = manager.GetNamespaceNames();
        }

        private void BindTypeNames(string nsName)
        {
            checkedListBox1.Items.Clear();
            foreach (var name in manager.GetTypeNames(nsName))
            {
                checkedListBox1.Items.Add(name);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IList<string> list = new List<string>();
            foreach (var item in checkedListBox1.CheckedItems)
            {
                list.Add(item.ToString());
            }

            textBox1.Text = manager.CreateSqlScript(comboBox1.SelectedValue.ToString(), list.ToArray());
            if (textBox1.Text.Length > 0)
            {
                tabControl1.SelectedTab = tabPage2;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string sql = textBox1.Text.TrimEnd();
            if (sql.Length == 0) return;
            ;
            DbManager mgr = new DbManager();
            mgr.SetCommand(sql).ExecuteNonQuery();
            MessageBox.Show("表创建完成");
        }
    }
}
