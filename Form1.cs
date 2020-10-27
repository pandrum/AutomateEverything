﻿using BL;
using Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace AutomateEverything
{
    public partial class MainWindow : Form
    {
        private PodcastController podcastController;
        private EpisodeController episodeController;
        private int selectedPodcast = 0;

        public MainWindow()
        {
            podcastController = new PodcastController();
            episodeController = new EpisodeController();
            InitializeComponent();
            FillPodcastFeed();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text;
            string name = txtName.Text;
            string category = cbCategory.Text;
            int interval = Convert.ToInt32(cbUpdate.SelectedItem);

            podcastController.AddNewPodcast(url, name, category, interval);
            FillPodcastFeed();
        }

        private void FillPodcastFeed()
        {
            var podcastList = podcastController.GetAllPodcasts();

            foreach (var podcast in podcastList)
            {
                dgPodcastFeed.Rows.Add(podcast.Name, podcast.Interval, podcast.Category);
            }
        }

        private void dgPodcastFeed_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            lbxEpisodes.Items.Clear();
            int selectedRow = dgPodcastFeed.CurrentCell.RowIndex;

            List<Episode> episodeList = episodeController.GetAllEpisodesFromPodcast(selectedRow);

            foreach (Episode episode in episodeList)
            {
                lbxEpisodes.Items.Add(episode.Name);
            }
            selectedPodcast = selectedRow;
        }

        private void lbxEpisodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedEpisodeName = lbxEpisodes.SelectedItem.ToString();
            List<Episode> episodeList = episodeController.GetAllEpisodesFromPodcast(selectedPodcast);

            foreach (Episode episode in episodeList)
            {
                if (selectedEpisodeName.Equals(episode.Name))
                {
                    txtEpisodeDescription.Text = episode.Description;
                }
            }
        }

        private void btnDeletePodcast_Click(object sender, EventArgs e)
        {
            int rowindex = dgPodcastFeed.CurrentCell.RowIndex;
            int columnindex = dgPodcastFeed.CurrentCell.ColumnIndex;
            var podcastName = dgPodcastFeed.Rows[rowindex].Cells[columnindex].Value.ToString();

            MessageBox.Show("Are you sure you want to delete the podcast " + podcastName + "?");
            podcastController.DeletePodcast(selectedPodcast);
            FillPodcastFeed();
        }
    }
}