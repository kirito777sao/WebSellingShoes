﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebSellingShoes.Models;
using WebSellingShoes.Repository;
using WebSellingShoes.Models;
using WebSellingShoes.Repository;

namespace Shopping_Tutorial.Areas.Admin.Controllers
{
    [Area("admin")]
    [Route("admin/user")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;



        public UserController(DataContext context,
            UserManager<AppUserModel> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {

            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;

        }
        [HttpGet]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            var usersWithRoles = await (from u in _dataContext.Users
                                        join ur in _dataContext.UserRoles on u.Id equals ur.UserId into userRoles
                                        from ur in userRoles.DefaultIfEmpty()
                                        join r in _dataContext.Roles on ur.RoleId equals r.Id into roles
                                        from r in roles.DefaultIfEmpty()
                                        select new { User = u, RoleName = r != null ? r.Name : "No Role" })
                                   .ToListAsync();

            return View(usersWithRoles);
        }
        [HttpGet]
        [Route("create")]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(new AppUserModel());
        }
        [HttpGet]
        [Route("edit")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("edit")]
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existingUser = await _userManager.FindByIdAsync(id);//lấy user dựa vao id
            if (existingUser == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update other user properties (excluding password)
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;

                var updateUserResult = await _userManager.UpdateAsync(existingUser); //thực hiện update
                if (updateUserResult.Succeeded)
                {
                    // Update user roles

                    var currentRoles = await _userManager.GetRolesAsync(existingUser);

                    var newRole = await _roleManager.FindByIdAsync(user.RoleId);

                    if (newRole != null)

                    {

                        if (!currentRoles.Contains(newRole.Name))

                        {

                            await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);

                            await _userManager.AddToRoleAsync(existingUser, newRole.Name);

                        }

                    }

                    TempData["success"] = "User đã được cập nhật thành công";
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    AddIdentityErrors(updateUserResult);
                    return View(existingUser);
                }
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            // Model validation failed
            TempData["error"] = "Model validation failed.";
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            string errorMessage = string.Join("\n", errors);

            return View(existingUser);
        }
        private void AddIdentityErrors(IdentityResult identityResult)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("create")]
        public async Task<IActionResult> Create(AppUserModel user)
        {
            if (ModelState.IsValid)
            {
                var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash); //tạo user
                if (createUserResult.Succeeded)
                {
                    var createUser = await _userManager.FindByEmailAsync(user.Email); //tìm user dựa vào email
                    var userId = createUser.Id; // lấy user Id
                    var role = _roleManager.FindByIdAsync(user.RoleId); //lấy RoleId
                                                                        //gán quyền
                    var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Result.Name);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var error in createUserResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    TempData["success"] = "User đã được tạo thành công";
                    return RedirectToAction("Index", "User");
                }
                else
                {

                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(user);
                }

            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");
            return View(user);

        }

        [HttpGet]
        [Route("delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                return View("Error");
            }
            TempData["success"] = "User đã được xóa thành công";
            return RedirectToAction("Index");
        }

    }
}
