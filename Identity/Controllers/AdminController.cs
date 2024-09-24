﻿using Azure.Identity;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Identity.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private IPasswordHasher<AppUser> _passwordHasher;
        private IPasswordValidator<AppUser> _passwordValidator;
        private IUserValidator<AppUser> _userValidator;

        public AdminController(UserManager<AppUser> userManager,IPasswordHasher<AppUser>passwordHasher,IPasswordValidator<AppUser>passwordValidator,IUserValidator<AppUser>userValidator)
        {
            _userManager = userManager;
            _passwordHasher = passwordHasher;
            _passwordValidator = passwordValidator;
            _userValidator = userValidator;
        }
        public IActionResult Index()
        {
            return View(_userManager.Users);
        }
        [HttpPost]
        public IActionResult Create()
        {
            return View();  
        }
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid )
            {
                AppUser appUser = new AppUser()
                {
                    UserName=user.Name,
                    Email=user.Email,
                };
                IdentityResult result=await _userManager.CreateAsync(appUser,user.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else 
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("",error.Description);
                    }
                }
            }
            return View(user);
        }
        public async Task<IActionResult> Update(string id)
        {
            AppUser user=await _userManager.FindByIdAsync(id);
            if (user != null)

                return View(user);

            else
            {
                return RedirectToAction("Index");
            }
           
        }
        //[HttpPost]
        //public async Task<IActionResult> Update(string id, string email, string password)
        //{
        //    AppUser user = await _userManager.FindByIdAsync(id);
        //    if (user != null)
        //    {
        //        if (!string.IsNullOrEmpty(email))
        //            user.Email = email;
        //        else
        //            ModelState.AddModelError("", "Email cannot be empty");

        //        if (!string.IsNullOrEmpty(password))
        //            user.PasswordHash = _passwordHasher.HashPassword(user, password);
        //        else
        //            ModelState.AddModelError("", "Password cannot be empty");

        //        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        //        {
        //            IdentityResult result = await _userManager.UpdateAsync(user);
        //            if (result.Succeeded)
        //                return RedirectToAction("Index");
        //            else
        //                Errors(result);
        //        }
        //    }
        //    else
        //        ModelState.AddModelError("", "User Not Found");
        //    return View(user);
        //}
        [HttpPost]
        public async Task<IActionResult> Update(string id, string email, string password)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult validEmail = null;
                if (!string.IsNullOrEmpty(email))
                {
                    validEmail = await _userValidator.ValidateAsync(_userManager, user);
                    if (validEmail.Succeeded)
                        user.Email = email;
                    else
                        Errors(validEmail);
                }
                else
                    ModelState.AddModelError("", "Email cannot be empty");

                IdentityResult validPass = null;
                if (!string.IsNullOrEmpty(password))
                {
                    validPass = await _passwordValidator.ValidateAsync(_userManager, user, password);
                    if (validPass.Succeeded)
                        user.PasswordHash = _passwordHasher.HashPassword(user, password);
                    else
                        Errors(validPass);
                }
                else
                    ModelState.AddModelError("", "Password cannot be empty");

                if (validEmail != null && validPass != null && validEmail.Succeeded && validPass.Succeeded)
                {
                    IdentityResult result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                        return RedirectToAction("Index");
                    else
                        Errors(result);
                }
            }
            else
                ModelState.AddModelError("", "User Not Found");

            return View(user);
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return View("Index", _userManager.Users);
        }
    }
}
