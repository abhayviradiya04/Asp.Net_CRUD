using Microsoft.AspNetCore.Mvc;
using ProductManagement.Models;
using System.Data.SqlClient;

namespace SimpleCrud.Controllers
{
    public class ProductController : Controller
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _env;

        public ProductController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            var list = new List<Product>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                string sql = @"SELECT p.Id, p.Name, p.Price, p.Image, c.Name AS CategoryName
                               FROM Product p
                               JOIN Category c ON p.CategoryId = c.Id";
                SqlCommand cmd = new SqlCommand(sql, con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new Product
                    {
                        Id = (int)dr["Id"],
                        Name = dr["Name"].ToString(),
                        Price = (decimal)dr["Price"],
                        Image = dr["Image"].ToString(),
                        CategoryName = dr["CategoryName"].ToString()
                    });
                }
            }
            return View(list);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = GetCategories();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product p)
        {
            string? fileName = null;

            // ✅ Upload Image if Provided
            if (p.ImageFile != null)
            {
                string uploadPath = Path.Combine(_env.WebRootPath, "images");
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(p.ImageFile.FileName);
                string filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    p.ImageFile.CopyTo(stream);
                }
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Product(Name,Price,Image,CategoryId) VALUES(@Name,@Price,@Image,@CategoryId)", con);
                cmd.Parameters.AddWithValue("@Name", p.Name);
                cmd.Parameters.AddWithValue("@Price", p.Price);
                cmd.Parameters.AddWithValue("@Image", fileName ?? "");
                cmd.Parameters.AddWithValue("@CategoryId", p.CategoryId);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Product p = new();
            ViewBag.Categories = GetCategories();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Product WHERE Id=@Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    p.Id = (int)dr["Id"];
                    p.Name = dr["Name"].ToString();
                    p.Price = (decimal)dr["Price"];
                    p.Image = dr["Image"].ToString();
                    p.CategoryId = (int)dr["CategoryId"];
                }
            }
            return View(p);
        }

        [HttpPost]
        public IActionResult Edit(Product p)
        {
            string? fileName = p.Image;

            if (p.ImageFile != null)
            {
                string uploadPath = Path.Combine(_env.WebRootPath, "images");
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(p.ImageFile.FileName);
                string filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    p.ImageFile.CopyTo(stream);
                }
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Product SET Name=@Name, Price=@Price, Image=@Image, CategoryId=@CategoryId WHERE Id=@Id", con);
                cmd.Parameters.AddWithValue("@Name", p.Name);
                cmd.Parameters.AddWithValue("@Price", p.Price);
                cmd.Parameters.AddWithValue("@Image", fileName ?? "");
                cmd.Parameters.AddWithValue("@CategoryId", p.CategoryId);
                cmd.Parameters.AddWithValue("@Id", p.Id);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Product WHERE Id=@Id", con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        private List<Category> GetCategories()
        {
            var list = new List<Category>();
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Category", con);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new Category
                    {
                        Id = (int)dr["Id"],
                        Name = dr["Name"].ToString()
                    });
                }
            }
            return list;
        }
    }
}
