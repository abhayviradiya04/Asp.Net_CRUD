

using System.Data;
using CRUDOprationPractise.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CRUDOprationPractise.Controllers
{
    public class EmployeeController : Controller
    {   
        private readonly IConfiguration configuration;

        public EmployeeController(IConfiguration _configuration)
        {
            configuration = _configuration;
        }
        public IActionResult View()
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("PR_Employee_SelectALL",connection);
            command.CommandType=CommandType.StoredProcedure;
            SqlDataReader reader = command.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(reader);


            return View(dt);
        }
        [HttpGet]
        public IActionResult AddOrEdit(int? id)
        {
            if (id == null)
            {
                return View(new EmployeeModel());
            }
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using SqlConnection connection=new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("PR_Employee_SelecyByPK", connection);
            command.CommandType=CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@id", id);
            SqlDataReader reader=command.ExecuteReader();
            EmployeeModel model=new EmployeeModel();
            if (reader.Read())
            {
                model.Eid = Convert.ToInt32(reader["Eid"]);
                model.Ename = reader["Ename"].ToString();
                model.Salary = Convert.ToDouble(reader["Salary"]);
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult Save(EmployeeModel model) {
            if (!ModelState.IsValid)
            {
                return View("AddOrEdit", model);
            }
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand(model.Eid==0 ? "PR_Employee_Insert":"Pr_Employee_Update",connection);
            command.CommandType=CommandType.StoredProcedure;
            if (model.Eid != 0)
            {
                command.Parameters.AddWithValue("id",model.Eid);
                
            }
            command.Parameters.AddWithValue("name", model.Ename);
            command.Parameters.AddWithValue("salary", model.Salary);
            command.ExecuteNonQuery();

            return RedirectToAction("View");
        }
        public IActionResult Delete(int id)
        {
            string connectionString = configuration.GetConnectionString("ConnectionString");
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("PR_Employee_Delete", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
            return RedirectToAction("View");
        }
    }
}
