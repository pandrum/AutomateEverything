﻿using BL;
using DL;
using DL.Exceptions;
using Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace AutomateEverything
{
    public partial class MainWindow : Form
    {
        private PodcastController podcastController;
        private EpisodeController episodeController;
        private CategoryController categoryController;
        private int selectedPodcast = 0;
        private List<Podcast> podcasts;

        public MainWindow()
        {
            InitializeComponent();
            podcastController = new PodcastController();
            episodeController = new EpisodeController();
            categoryController = new CategoryController();
            podcasts = podcastController.GetPodcasts();
            FillPodcastList();
            FillCategoryList();
            FillCategoryComboBox();
            InitTimers();
        }

        private async void btnAddNewPodcast_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text;
            string name = txtName.Text;
            string category = cbCategory.Text;
            int interval = Convert.ToInt32(cbInterval.SelectedItem);

            if (Validator.CheckTextField(txtUrl, txtName) && Validator.CheckCombobox(cbCategory, cbInterval) && Validator.CheckIfValidURL(url) && Validator.CheckDuplicatePodcast(url))
            {
                try
                {
                    await podcastController.AddNewPodcast(url, name, category, interval);
                    FillPodcastList();
                    ClearInputs();
                    InitTimers(url, interval);
                    MessageBox.Show("Podcast added!");
                }
                catch (EmptyTextFieldException)
                {
                    throw;
                }
            }
        }

        private void btnUpdatePodcast_Click(object sender, EventArgs e)
        {
            string url = txtUrl.Text;
            string name = txtName.Text;
            string interval = cbInterval.Text;
            string category = cbCategory.Text;
            if (Validator.CheckTextField(txtName, txtUrl) && Validator.CheckIfValidURL(url))
            {
                podcastController.UpdateAllPodcastInfo(selectedPodcast, url, name, interval, category);
                FillPodcastList();
                MessageBox.Show("Selected podcast updated!");
            }
        }

        private void btnDeletePodcast_Click(object sender, EventArgs e)
        {
            if (dgPodcastFeed.CurrentCell != null)
            {
                int rowindex = dgPodcastFeed.CurrentCell.RowIndex;
                int columnindex = 1;
                var podcastName = dgPodcastFeed.Rows[rowindex].Cells[columnindex].Value.ToString();

                DialogResult response = MessageBox.Show("Are you sure you want to delete the podcast " + podcastName + "?", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (response == DialogResult.OK)
                {
                    podcastController.DeletePodcast(selectedPodcast);
                    FillPodcastList();
                    ClearInputs();
                    ClearEpisodesList();
                }
            }
        }

        private void btnAddCategory_Click(object sender, EventArgs e)
        {
            string categoryName = txtCategory.Text;
            if (Validator.CheckCategoryIsNotEmpty(txtCategory) && Validator.CheckDuplicateCategory(categoryName))
            {
                Category category = new Category(categoryName);
                categoryController.AddNewCategory(category);
                FillCategoryList();
                FillCategoryComboBox();
                txtCategory.Text = string.Empty;
                MessageBox.Show("New category added!");
            }
        }

        private void btnUpdateCategory_Click(object sender, EventArgs e)
        {
            if (Validator.CheckCategoryIsNotEmpty(txtCategory) && Validator.CheckIfCategoryItemSelected(lbxCategories))
            {
                try
                {
                    string currentName = lbxCategories.SelectedItem.ToString();
                    string newName = txtCategory.Text;

                    podcastController.UpdatePodcastCategory(currentName, newName);
                    categoryController.UpdateCategoryName(currentName, newName);

                    FillPodcastList();
                    FillCategoryList();
                    FillCategoryComboBox();
                    txtCategory.Text = string.Empty;
                    MessageBox.Show("Category updated!");
                }
                catch (Exception)
                {
                    Console.WriteLine("Out of bounds for categories!");
                }
            }
        }

        private void btnDeleteCategory_Click(object sender, EventArgs e)
        {
            if (Validator.CheckIfCategoryItemSelected(lbxCategories))
            {
                string categoryName = lbxCategories.SelectedItem?.ToString();

                DialogResult res = MessageBox.Show("Are you sure you want to delete the category " + categoryName + "?", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (res == DialogResult.OK)
                {
                    categoryController.DeleteCategory(categoryName);
                    podcastController.DeletePodcastByCategory(categoryName);
                    FillPodcastList();
                    FillCategoryList();
                    FillCategoryComboBox();
                    txtCategory.Text = string.Empty;
                    ClearEpisodesList();
                    ClearInputs();
                }
            }
        }

        private void dgPodcastFeed_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            lbxEpisodes.Items.Clear();
            if (dgPodcastFeed.CurrentRow != null)
            {
                int selectedPodcast = dgPodcastFeed.CurrentRow.Index;
                PopulateTextBoxes(selectedPodcast);
                var episodeList = episodeController.GetAllEpisodesFromPodcast(selectedPodcast);
                episodeList.ToList().ForEach(episode => lbxEpisodes.Items.Add(episode.Name));

                this.selectedPodcast = selectedPodcast;

                lblEpisodeList.Text = "Episodes for " + podcastController.GetPodcastName(selectedPodcast);
            }
        }

        private void lbxEpisodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string episodeName = lbxEpisodes.SelectedItem.ToString();
            lblEpisodeDescription.Text = episodeName;

            string selectedEpisodeName = lbxEpisodes.SelectedItem.ToString();
            var episodeList = episodeController.GetAllEpisodesFromPodcast(selectedPodcast);

            foreach (var episode in episodeList.Where(ep => ep.Name.Equals(selectedEpisodeName)))
            {
                txtEpisodeDescription.Text = episode.Description;
            }
        }

        private void lbxCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgPodcastFeed.Rows.Clear();
            var selectedCategory = lbxCategories.SelectedItem?.ToString();
            var podcastList = podcastController.GetPodcasts();

            foreach (var podcast in podcastList.Where(p => p.Category.Equals(selectedCategory)))
            {
                dgPodcastFeed.Rows.Add(podcast.TotalEpisodes, podcast.Name, podcast.Interval, podcast.Category);
            }
        }

        private void InitTimers()
        {
            int interval = 0;
            foreach (var podcast in podcasts)
            {
                var timer = new Timer();

                timer.Tag = podcast.Url;
                if (podcast.Interval == 1)
                {
                    interval = 60000;
                }
                if (podcast.Interval == 2)
                {
                    interval = 120000;
                }
                if (podcast.Interval == 3)
                {
                    interval = 180000;
                }
                timer.Interval = interval;
                timer.Tag = podcast.Url;
                timer.Enabled = true;
                timer.Tick += Timer_Tick;
                timer.Start();
            }
        }

        private void InitTimers(string url, int interval)
        {
            var timer = new Timer();
            if (interval == 1)
            {
                interval = 60000;
            }
            if (interval == 2)
            {
                interval = 120000;
            }
            if (interval == 3)
            {
                interval = 180000;
            }
            timer.Interval = interval;
            timer.Tag = url;
            timer.Enabled = true;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Timer timer = (Timer)sender;
            string url = (string)timer.Tag;
            podcastController.CheckForNewEpisodes(url);
            dgPodcastFeed.Rows.Clear();
            podcasts = podcastController.GetPodcasts();
            FillPodcastList();
        }

        private void PopulateTextBoxes(int selectedRow)
        {
            var url = podcastController.GetPodcastUrl(selectedRow);
            txtUrl.Text = url;

            var name = podcastController.GetPodcastName(selectedRow);
            txtName.Text = name;

            var interval = podcastController.GetPodcastUpdateInterval(selectedRow);
            cbInterval.SelectedItem = interval;

            var category = podcastController.GetPodcastCategory(selectedRow);
            cbCategory.Text = category;
        }

        private void FillPodcastList()
        {
            dgPodcastFeed.Rows.Clear();
            dgPodcastFeed.Refresh();
            var podcastList = podcastController.GetPodcasts();
            podcastList.ToList().ForEach(podcast => dgPodcastFeed.Rows.Add(podcast.TotalEpisodes, podcast.Name, podcast.Interval, podcast.Category));
        }

        private void FillCategoryComboBox()
        {
            cbCategory.Items.Clear();
            var categoryList = categoryController.GetCategories();
            categoryList.ToList().ForEach(category => cbCategory.Items.Add(category.Name));
        }

        private void FillCategoryList()
        {
            lbxCategories.Items.Clear();
            var categoryList = categoryController.GetCategories();
            categoryList.ToList().ForEach(category => lbxCategories.Items.Add(category.Name));
        }

        private void ClearInputs()
        {
            txtUrl.Text = "";
            txtName.Text = "";
            cbInterval.SelectedIndex = -1;
            cbCategory.SelectedIndex = -1;
        }

        private void ClearEpisodesList()
        {
            lbxEpisodes.Items.Clear();
            txtEpisodeDescription.Text = string.Empty;
        }
    }
}