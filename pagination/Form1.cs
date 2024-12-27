using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace pagination
{
    public partial class Form1 : Form
    {
        private int pagenumber = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pagenumber = 1;
            Call_Page();
            Btn_Previous.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pagenumber = pagenumber + 1;
            Call_Page();
            if (pagenumber > 1)
            {
                Btn_Previous.Enabled = true;
            }
        }

        private async void Call_Page()
        {
            dataGridView1.DataSource = null;
            
            // Define the base URL of the API
            string baseUrl = "http://localhost:5000/articles";
            string endpoint = "?page=" + pagenumber; // Example endpoint

            using (HttpClient client = new HttpClient())
            {
                // Configure HttpClient
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "C# Console App");


                int maxRetries = 10; // Maximum number of retries
                int retryCount = 0; // Current retry count
                bool success = false;

                while (retryCount < maxRetries && !success)
                {
                    try
                    {
                        // Make an asynchronous GET request
                        HttpResponseMessage response = await client.GetAsync(endpoint);

                        // Ensure the response is successful
                        response.EnsureSuccessStatusCode();

                        if (response.IsSuccessStatusCode)
                        {
                            success = true;
                            // Read and deserialize the response content
                            string responseData = await response.Content.ReadAsStringAsync();
                            var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseData);

                            // Process and display the data
                            if (apiResponse?.Data != null)
                            {
                                DataTable dataTable = new DataTable();
                                dataTable.Columns.Add("ID", typeof(int));
                                dataTable.Columns.Add("Title", typeof(string));
                                dataTable.Columns.Add("Content", typeof(string));

                                foreach (var article in apiResponse.Data)
                                {
                                    dataTable.Rows.Add(article.Id, article.Title, article.Content);
                                }

                                dataGridView1.DataSource = dataTable;

                                if (apiResponse.IsNext)
                                {
                                    btn_Next.Enabled = true;
                                }
                                else
                                { btn_Next.Enabled = false; }

                                label1.Text = "Page No: " + apiResponse.Page.ToString();

                                Console.WriteLine($"Next Page Available: {apiResponse.IsNext}");
                                Console.WriteLine($"Current Page: {apiResponse.Page}, Per Page: {apiResponse.PerPage}");
                            }
                            else
                            {
                                Console.WriteLine("No data available in the response.");
                            }
                        }
                        else
                        {

                        }

                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"An error occurred: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unexpected error: {ex.Message}");
                    }
                }


                
            }
        }

        private void Btn_Previous_Click(object sender, EventArgs e)
        {
            pagenumber = pagenumber - 1;
            Call_Page();
            if (pagenumber == 1)
            {
                Btn_Previous.Enabled = false;
            }
        }
    }

    public class Article
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

    public class ApiResponse
    {
        [JsonPropertyName("data")]
        public List<Article> Data { get; set; }

        [JsonPropertyName("is_next")]
        public bool IsNext { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }
    }

}
