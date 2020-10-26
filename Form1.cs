﻿using BL;
using System;

using System.Windows.Forms;

namespace AutomateEverything
{
    public partial class MainWindow : Form
    {
        private PodcastController podcastController;

        public MainWindow()
        {
            InitializeComponent();
            podcastController = new PodcastController();
            FillTables();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text;
            string name = txtName.Text;
            string category = cbCategory.SelectedItem.ToString();
            int interval = Convert.ToInt32(cbUpdate.SelectedItem);

            podcastController.AddNewPodcast(url, name, category, interval);
        }

        public void FillTables()
        {
            try
            {
                var podcastList = podcastController.GetAllPodcasts();

                foreach (var p in podcastList)
                {
                    dgPodcastFeed.Rows.Add(p.Name, p.Interval, p.Category);
                    Console.WriteLine(p.Name, p.Interval, p.Category);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Fel!");
            }
        }
    }
}