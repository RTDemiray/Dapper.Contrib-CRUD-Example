using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Dapper.Contrib.Extensions;
using System.Data;
using Microsoft.AspNetCore.Http;
using Dapper.Contrib_CRUD_Example.Models;

namespace Dapper.Contrib_CRUD_Example.Controllers
{
    public class HomeController : Controller
    {
        //Local DB için ayarlanmıştır! Lütfen back-up veritabanı dosyasını yükleyin!
        protected const string sqlString = @"Server=.; Initial Catalog=ServerSideDB; Integrated Security=true";
        //IRepository<Users> _repoUsers;

        //public HomeController(IRepository<Users> repoUsers)
        //{
        //    _repoUsers = repoUsers;
        //}

        public async Task<IActionResult> Index()
        {
            //var deneme = await _repoUsers.GetAllAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadTable(IFormCollection formData)
        {
            try
            {
                //Özelleştirilebilir verileri çekiyoruz.
                string search = formData["search[value]"][0]; //Arama kutusuna girilen string değer.
                string draw = formData["draw"][0]; //Tabloyu yeniden çizer (oluşturur).
                string order = formData["order[0][column]"][0]; //Sıralamanın uygulanması gerek sütun.
                string orderDir = formData["order[0][dir]"][0]; //Sırasıyla artan veya azalan sıralamayı belirtmek için asc veya desc olacaktır.
                int startRec = Convert.ToInt32(formData["start"][0]); //Kaçıncı veriden itibaren başlayacağını belirtir.
                int pageSize = Convert.ToInt32(formData["length"][0]); //Sayfalama sayısını alıyoruz. 

                //Sql bağlantısı kuruyoruz.
                using var con = new SqlConnection(sqlString);

                //Listeyi tanımlıyoruz.
                IEnumerable<Users> data = null;

                int totalRecords, activeRecords, passiveRecords, recFilter;

                //Toplam kayıt sayısını tutuyoruz.
                totalRecords = await con.ExecuteScalarAsync<int>("Select count(*) from Users");
                //Aktif kayıt sayısını tutuyoruz.
                activeRecords = await con.ExecuteScalarAsync<int>("Select count(*) from Users where is_active=1");
                //Pasif kayıt sayısını tutuyoruz.
                passiveRecords = await con.ExecuteScalarAsync<int>("Select count(*) from Users where is_active=0");

                //Aranan kelimeye boş değilse
                if (!string.IsNullOrEmpty(search) || !string.IsNullOrWhiteSpace(search))
                {
                    // QueryAsync methodu ile aranan kelimeyi tüm kolonlarda arıyor. (Dapper methodudur!)
                    data = await con.QueryAsync<Users>($"Select * from Users where LOWER(first_name) LIKE '%{ search.ToLower() }%' or LOWER(last_name) LIKE '%{ search.ToLower() }%' or LOWER(email) LIKE '%{ search.ToLower() }%' or LOWER(gender) LIKE '%{ search.ToLower() }%' or LOWER(ip_address) LIKE '%{ search.ToLower() }%' or CONVERT(varchar,date_time,103) LIKE '%{string.Format("{0: dd/MM/yyyy}", search) }%' or LOWER(is_active) {(search.ToLower().Contains("aktif") ? "=1" : search.ToLower().Contains("pasif") ? "=0" : "=null")}");
                    recFilter = data.Count(); // Filtrelenen kayıt sayısını tutuyoruz.
                    data = data.Skip(startRec).Take(pageSize); // Son olarak sayfalama işlemi yapıyoruz.
                }
                else
                {
                    string startRecStr = startRec.ToString().Substring(0, 1); // String değer göndermemiz gerekiyor ve sayının sadece ilk hanesini almamız gerekiyor.
                    // QueryAsync methodu ile tüm kayıtlar arasında sayfalama işlemini yapıyoruz. (Dapper methodudur!)
                    data = await con.QueryAsync<Users>("select * from Users Order by id OFFSET @startRecStr * @pageSize ROWS FETCH NEXT @pageSize ROWS ONLY", new { pageSize = pageSize, startRecStr = startRecStr });
                    recFilter = totalRecords; // Toplam kayıt sayısını değişkene atıyoruz.
                }
                
                //Sıralama işlemini yapıyoruz
                if (!(string.IsNullOrEmpty(order) && string.IsNullOrEmpty(orderDir)))
                {
                    data = SortTableData(order, orderDir, data);
                }

                //Elde ettiğimiz verileri yeni bir liste içine atıyoruz.
                var allData = data.Select(users =>
                new
                {
                    RowGuid = users.RowGuid,
                    Name = users.first_name,
                    LastName = users.last_name,
                    Email = users.email,
                    Gender = users.gender,
                    IpAddress = users.ip_address,
                    Date = users.date_time,
                    IsActive = users.is_active
                }).ToList();
                //Verileri json formatta döndürüyoruz.
                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords, //Tabloda ki toplam veri sayısı.
                    recordsFiltered = recFilter, //Toplam filtrelenmiş veri sayısı.
                    data = allData, //Datatable işlenecek veri.
                    activeRecords = activeRecords, //Aktif kayıt sayısı.
                    passiveRecords = passiveRecords //Toplam kayıt sayısı.
                });

            }
            catch (Exception ex)
                {
                return Json(new {ex.Message });
            }
        }

        private IEnumerable<Users> SortTableData(string order, string orderDir, IEnumerable<Users> data)
        {
            // datatable jquery tarafında sıralamayı ayarlıyoruz.
            IEnumerable<Users> lst = null;
            try
            {
                switch (order)
                {
                    case "0":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.first_name).ToList()
                                                                                                 : data.OrderBy(p => p.first_name).ToList();
                        break;
                    case "1":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.last_name).ToList()
                                                                                                 : data.OrderBy(p => p.last_name).ToList();
                        break;
                    case "2":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.email).ToList()
                                                                                                 : data.OrderBy(p => p.email).ToList();
                        break;
                    case "3":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.gender).ToList()
                                                                                                 : data.OrderBy(p => p.gender).ToList();
                        break;
                    case "4":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.ip_address).ToList()
                                                                                                   : data.OrderBy(p => p.ip_address).ToList();
                        break;
                    case "5":
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.date_time).ToList()
                                                                                                   : data.OrderBy(p => p.date_time).ToList();
                        break;
                    default:
                        lst = orderDir.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? data.OrderByDescending(p => p.is_active).ToList()
                                                                                                 : data.OrderBy(p => p.is_active).ToList();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return lst;
        }

        public async Task<JsonResult> ActiveControl(Guid rowGuid)
        {
            //Sql bağlantısı kuruyoruz.
            using var con = new SqlConnection(sqlString);

            //parametreli ilk kaydı çeken sorgu (Dapper.Contrib methodudur!) 
            Users user = await con.GetAsync<Users>(rowGuid);

            //parametreli ilk kaydı çeken sorgu (dapper methodudur!) 
            //Users user = await con.QueryFirstAsync<Users>($"Select * from Users Where RowGuid='{rowGuid}'");
            if (user.is_active)
            {
                user.is_active = false;
            }
            else
            {
                user.is_active = true;
            }
            //Parametresiz update sorgusu (Dapper.Contrib methodudur!)
            await con.UpdateAsync(user);

            //Parametreli update sorgusu (dapper methodudur!)
            //await con.ExecuteScalarAsync<Users>($"Update Users set is_active=@is_active where RowGuid=@rowGuid", new { is_active = user.is_active, RowGuid = user.RowGuid });
            return Json(new { IsActive = user.is_active });
        }

        public async Task<JsonResult> Delete(Guid rowGuid)
        {
            //Sql bağlantısı kuruyoruz.
            using var con = new SqlConnection(sqlString);

            //parametreli ilk kaydı çeken sorgu (Dapper.Contrib methodudur!)
            Users user = await con.GetAsync<Users>(rowGuid);

            //parametreli ilk kaydı çeken sorgu (dapper methodudur!) 
            //Users user = await con.QueryFirstAsync<Users>($"Select RowGuid from Users where RowGuid=@rowGuid", new { RowGuid = rowGuid });
            if (user == null)
            {
                return Json(new { IsActive = false });
            }
            else
            {
                //parametreli RowGuid değerine eş gelen kaydı silen sorgu (Dapper.Contrib methodudur!)
                await con.DeleteAsync(user);

                //parametreli RowGuid değerine eş gelen kaydı silen sorgu (dapper methodudur!)
                //await con.ExecuteScalarAsync<Users>($"Delete from Users where RowGuid=@rowGuid", new { RowGuid = user.RowGuid });
                return Json(new { IsActive = true });
            }
        }

        public async Task<PartialViewResult> CreateOrUpdate(Guid? RowGuid)
        {
            if(RowGuid.HasValue == false) //RowGuid değeri boş ise
            {
                return PartialView();
            }
            else
            {
                //Sql bağlantısı kuruyoruz.
                using var con = new SqlConnection(sqlString);

                //parametreli gelen RowGuid değeri ile ilk kaydı çeken sorgu (Dapper.Contrib methodudur!)
                Users user = await con.GetAsync<Users>(RowGuid);

                //parametreli gelen RowGuid değeri ile ilk kaydı çeken sorgu (dapper methodudur!)
                //Users user = await con.QueryFirstAsync<Users>("Select * from Users where RowGuid=@RowGuid", new { RowGuid = RowGuid });
                return PartialView(user);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateOrUpdate(Users users)
        {
            try
            {
                if (users.RowGuid.HasValue == false) //RowGuid değeri boş ise yeni kayıt ekle değilse var olan kaydı güncelle.
                {
                    users.RowGuid = Guid.NewGuid();
                    users.date_time = DateTime.Now;

                    //Sql bağlantısı kuruyoruz.
                    using var con = new SqlConnection(sqlString);

                    //Parametresiz insert sorgusu (Dapper.Contrib methodudur!)
                    await con.InsertAsync(users);

                    //Parametreli insert sorgusu (dapper methodudur!)
                    //await con.ExecuteScalarAsync<Users>("Insert into Users values(@RowGuid,@first_name,@last_name,@email,@gender,@ip_address,@is_active,@date_time)", new { RowGuid = users.RowGuid, first_name = users.first_name, last_name = users.last_name, email = users.email, gender = users.gender, ip_address = users.ip_address, is_active = users.is_active, date_time = users.date_time });
                    return Json(new { status = true, message = "Kayıt başarılı bir şekilde oluşturuldu!" });
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        //Sql bağlantısı kuruyoruz.
                        using var con = new SqlConnection(sqlString);

                        //Parametresiz update sorgusu (Dapper.Contrib methodudur!)
                        await con.UpdateAsync(users);

                        //Parametreli update sorgusu (dapper methodudur!)
                        //await con.ExecuteScalarAsync<Users>("Update Users set first_name=@first_name,last_name=@last_name,email=@email,gender=@gender,ip_address=@ip_address,is_active=@is_active,date_time=@date_time where RowGuid=@RowGuid", new { RowGuid = users.RowGuid, first_name = users.first_name, last_name = users.last_name, email = users.email, gender = users.gender, ip_address = users.ip_address, is_active = users.is_active, date_time = users.date_time });
                        return Json(new { status = true, message = "Kayıt başarılı bir şekilde güncellendi!" });
                    }
                    else
                    {
                        return Json(new { status = false, message = "Kayıt güncellenemedi!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
            
        }
    }
}
