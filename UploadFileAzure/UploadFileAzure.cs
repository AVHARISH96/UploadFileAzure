using Azure.Storage.Blobs;
using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace UploadFileAzure
{
    public partial class UploadFileAzure : Form
    {
        public UploadFileAzure()
        {
            InitializeComponent();
        }
        private void btnUpload_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Files (*.*)|*.*";
            openFileDialog.Title = "Select a File to Upload";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                string fileName = Path.GetFileName(filePath);

                string connectionString = ConfigurationManager.AppSettings["connectionString"];
                string containerName = ConfigurationManager.AppSettings["containerName"];

                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                containerClient.CreateIfNotExists();

                BlobClient blobClient = containerClient.GetBlobClient(fileName);

                try
                {
                    using (FileStream fileStream = File.OpenRead(filePath))
                    {
                        blobClient.Upload(fileStream, true);
                    }

                    MessageBox.Show("File uploaded successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error uploading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
