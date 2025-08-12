using System.Data;
using CRUDOprationPractise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;

namespace CRUDOprationPractise.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IConfiguration configuration;
        public DepartmentController(IConfiguration _configuration)
        {
            configuration= _configuration;
        }
        public IActionResult View()
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using SqlConnection connection=new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("PR_Department_SelectALL",connection);
            command.CommandType = CommandType.StoredProcedure;
            SqlDataReader reader=command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);
            return View(dt);
        }
        [HttpGet]
        public IActionResult AddOrEdit(int? id)
        {
            DepartmentModel model = new DepartmentModel();

            // Fill dropdown data
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using SqlConnection con = new SqlConnection(connectionString);
            con.Open();

            // Get employee list
            SqlCommand empCmd = new SqlCommand("PR_Employee_Dropdown", con);
            empCmd.CommandType = CommandType.StoredProcedure;
            SqlDataReader empReader = empCmd.ExecuteReader();

            List<SelectListItem> empList = new List<SelectListItem>();
            while (empReader.Read())
            {
                empList.Add(new SelectListItem
                {
                    Value = empReader["Eid"].ToString(),
                    Text = empReader["Ename"].ToString()
                });
            }
            empReader.Close();
            ViewBag.EmployeeList = empList;


            if (id == null)
            {
                return View(new DepartmentModel());
            }
            
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("PR_Department_SelectByPK", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@id", id);
            SqlDataReader reader = command.ExecuteReader();
            
            if(reader.Read()){
                model.Did = Convert.ToInt32(reader["Did"]);
                model.Dname = reader["Dname"].ToString();
                model.Eid = Convert.ToInt32(reader["Eid"]);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Save(DepartmentModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("AddOrEdit", model);
            }
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand(model.Did == 0 ? "PR_Department_Insert" : "PR_Department_Update", connection);
            command.CommandType = CommandType.StoredProcedure;
            if (model.Did != 0)
            {
                command.Parameters.AddWithValue("id", model.Did);
            }
            command.Parameters.AddWithValue("name", model.Dname);
            command.Parameters.AddWithValue("eid", model.Eid);
            command.ExecuteNonQuery();
            return RedirectToAction("View");

        }
        public IActionResult Delete(int id)
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("PR_Department_Delete", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
            return RedirectToAction("View");


        }
    }
}
