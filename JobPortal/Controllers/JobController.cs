using JobPortal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NuGet.Protocol.Plugins;

namespace JobPortal.Controllers
{
    public class JobController : Controller
    {
        IConfiguration configuration;

        public JobController(IConfiguration configuration)
        {
            this.configuration= configuration;
        }
        // GET: JobController
        public ActionResult Index(string searchText,string searchType)
        {
            string connectionString = configuration.GetConnectionString("JOB-PORTAL");
            SqlConnection connection = new(connectionString);

            connection.Open();
            List<Job>jobList = new List<Job>();
            searchText ??= "";
            searchType ??= "";
           // string searchedText= (ViewBag.SearchText==null)?"": (string)ViewBag.SearchText;
            Console.WriteLine("search text"+searchText);
            string query = $"SELECT j.job_id,j.job_name,c.cat_name,s.sub_cat_name FROM JOB j JOIN CATEGORY c ON j.cat_id = c.cat_id  JOIN SUB_CATEGORY s ON j.sub_id=s.sub_id";
            SqlCommand command = new SqlCommand(query, connection);

            
            using(SqlDataReader dataReader = command.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    Job job = new Job();
                    job.job_id =Convert.ToInt32( dataReader["job_id"]);
                    job.job_name = (string)dataReader["job_name"];
                    job.cat_name =((string)dataReader["cat_name"]);
                    job.sub_name =((string)dataReader["sub_cat_name"]);

                    if ( searchType.Equals("name"))
                    {
                        if (searchText.Length != 0 && job.job_name.StartsWith(searchText))
                            jobList.Add(job);
                        if (searchText.Length == 0)
                            jobList.Add(job);
                    }
                    else if (searchType.Equals("category"))
                    {
                        if (searchText.Length != 0 && job.cat_name.StartsWith(searchText))
                            jobList.Add(job);
                        if (searchText.Length == 0)
                            jobList.Add(job);
                    }
                    else if (searchType.Equals("subCategory"))
                    {
                        if (searchText.Length != 0 && job.sub_name.StartsWith(searchText))
                            jobList.Add(job);
                        if (searchText.Length == 0)
                            jobList.Add(job);
                    }
                    else
                    {
                        jobList.Add(job);
                    }
             
                }
            }
            ViewBag.JobsList=jobList;
            connection.Close();
            return View();
        }
        [HttpPost]
        public ActionResult Index(IFormCollection collection)
        {
            ViewBag.SearchText = collection["search-text"];
            Console.WriteLine("search Text in post :" + collection["search-text"]);

            return RedirectToAction("Index", new { searchText=collection["search-text"], searchType = collection["search-type"] });
        }
        // GET: JobController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: JobController/Create
        public ActionResult Create()
        {
            string connectionString = configuration.GetConnectionString("JOB-PORTAL");
            SqlConnection connection = new(connectionString);


            connection.Open();

            List<KeyValuePair<int, string>> jobCategories = new List<KeyValuePair<int, string>>();
            List<KeyValuePair<int, string>> jobSubCategories = new List<KeyValuePair<int, string>>();

            string categoryFetchQuery = $"SELECT cat_id,cat_name from CATEGORY";
            string subCategoryFetchQuery = $"SELECT sub_id,sub_cat_name from SUB_CATEGORY";

            SqlCommand commandFetchCategory = new SqlCommand(categoryFetchQuery, connection);
            SqlCommand commandFetchSubCategory = new SqlCommand(subCategoryFetchQuery, connection);


            using (SqlDataReader dataReader = commandFetchCategory.ExecuteReader())
            {
                while (dataReader.Read())
                {

                    jobCategories.Add(new KeyValuePair<int, string>((int)dataReader["cat_id"], (string)dataReader["cat_name"]));

                }
            }
            using (SqlDataReader dataReader = commandFetchSubCategory.ExecuteReader())
            {
                while (dataReader.Read())
                {

                    jobSubCategories.Add(new KeyValuePair<int, string>((int)dataReader["sub_id"], (string)dataReader["sub_cat_name"]));

                }
            }
            ViewBag.CategoriesList = jobCategories;
            ViewBag.SubCategoriesList = jobSubCategories;

            connection.Close();
            return View();
        }

        // POST: JobController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try {
                string connectionString = configuration.GetConnectionString("JOB-PORTAL");
                SqlConnection connection = new(connectionString);


                connection.Open();

                //fetch all categories
               

                string query = $"INSERT INTO JOB VALUES({collection.ElementAt(0).Value},'{collection.ElementAt(1).Value}',{collection.ElementAt(2).Value},{collection.ElementAt(3).Value})";
                SqlCommand command = new SqlCommand(query, connection);

                Console.WriteLine(query);

                command.ExecuteNonQuery();

                ViewBag.ResultMessage = "Job Added Successfully";
                ViewBag.AlertCode = 1;

                return RedirectToAction("Index"); 
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewBag.ResultMessage = "Job Failed To Add";
                ViewBag.AlertCode = 0;


            }

            return View();
        }

        // GET: JobController/Edit/5
        public ActionResult Edit(int job_id)
        {
            List<Job> jobList = new List<Job>();
            string connectionString = configuration.GetConnectionString("JOB-PORTAL");
            SqlConnection connection = new(connectionString);

            connection.Open();


            string query = $"SELECT j.job_id,j.job_name,c.cat_name,s.sub_cat_name FROM JOB j JOIN CATEGORY c ON j.cat_id = c.cat_id  JOIN SUB_CATEGORY s ON j.sub_id=s.sub_id";
            SqlCommand command = new SqlCommand(query, connection);

            using (SqlDataReader dataReader = command.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    Job job = new Job();
                    job.job_id = Convert.ToInt32(dataReader["job_id"]);
                    job.job_name = (string)dataReader["job_name"];
                    job.cat_name = ((string)dataReader["cat_name"]);
                    job.sub_name = ((string)dataReader["sub_cat_name"]);

                    jobList.Add(job);


                }
            }
            ViewBag.JobsList = jobList;
            ViewBag.EditJobId = job_id;
            Console.WriteLine(job_id);
            connection.Close();

            return View("Index");
        }

        // POST: JobController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                string connectionString = configuration.GetConnectionString("JOB-PORTAL");
                SqlConnection connection = new(connectionString);


                connection.Open();

                string categoryName = collection.ElementAt(2).Value;
                string subCategoryName = collection.ElementAt(3).Value;

                //fetch categoryId and subCategoryID in queries

                string fetchCategoryIdQuery = $"select cat_id from category where cat_name='{categoryName}'";
                string fetchSubCategoryIdQuery = $"select sub_id from sub_category where sub_cat_name='{subCategoryName}'";

                SqlCommand commandFetchCategoryId = new(fetchCategoryIdQuery, connection);
                SqlCommand commandFetchSubCategoryId = new(fetchSubCategoryIdQuery, connection);

                int categoryId=-1,subCategoryId=-1;

                using (SqlDataReader dataReader = commandFetchCategoryId.ExecuteReader())
                {
                    while(dataReader.Read())
                        categoryId = (int)dataReader["cat_id"];
                }

                using (SqlDataReader dataReader = commandFetchSubCategoryId.ExecuteReader())
                {
                    while (dataReader.Read())

                        subCategoryId = (int)dataReader["sub_id"];
                }

                //Console.WriteLine(categoryId + "---" + subCategoryId);



                string query = $"UPDATE JOB set job_name='{collection.ElementAt(1).Value}',cat_id={categoryId},sub_id={subCategoryId} where job_id={collection.ElementAt(0).Value}";
                SqlCommand command = new SqlCommand(query, connection);

                Console.WriteLine(query);

                command.ExecuteNonQuery();

                ViewBag.ResultMessage = "Job Added Successfully";
                ViewBag.AlertCode = 1;

               return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ViewBag.ResultMessage = "Job Failed To Add";
                ViewBag.AlertCode = 0;
            }
            return RedirectToAction("Index");
        }

        // GET: JobController/Delete/5
        public ActionResult Delete(int job_id)
        {
            Console.WriteLine("Delete called with Job Id " + job_id);
            List<Job> jobList = new List<Job>();
            string connectionString = configuration.GetConnectionString("JOB-PORTAL");
            SqlConnection connection = new(connectionString);

            connection.Open();


            string query = $"SELECT j.job_id,j.job_name,c.cat_name,s.sub_cat_name FROM JOB j JOIN CATEGORY c ON j.cat_id = c.cat_id  JOIN SUB_CATEGORY s ON j.sub_id=s.sub_id";
            SqlCommand command = new SqlCommand(query, connection);

            using (SqlDataReader dataReader = command.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    Job job = new Job();
                    job.job_id = Convert.ToInt32(dataReader["job_id"]);
                    job.job_name = (string)dataReader["job_name"];
                    job.cat_name = ((string)dataReader["cat_name"]);
                    job.sub_name = ((string)dataReader["sub_cat_name"]);


                    jobList.Add(job);


                }
            }
            ViewBag.JobsList = jobList;

            return View("Index");
        }

        // POST: JobController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int jobId, IFormCollection collection)
        {
            Console.WriteLine("delete post with id " + jobId);

            try
            {
                string connectionString = configuration.GetConnectionString("JOB-PORTAL");
                SqlConnection connection = new(connectionString);

                connection.Open();

                string query = $"delete from job where job_id={jobId}";

                Console.WriteLine(query);

                SqlCommand command=new SqlCommand(query, connection);

                command.ExecuteNonQuery();

                return RedirectToAction("Index");


            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);

            }


                return RedirectToAction("Index");
        }
    }
}
