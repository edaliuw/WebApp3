﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using tourneybracket.Data;
using tourneybracket.Models;
using Microsoft.Extensions.Configuration.Binder;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace tourneybracket.Controllers
{
    public class BracketsController : Controller
    {
        private readonly BracketContext _context;

        public BracketsController(BracketContext context)
        {
            _context = context;
        }

        // GET: Brackets
        public async Task<IActionResult> Index()
        {
            return View(await _context.Brackets.ToListAsync());
        }

        // GET: Brackets/Details/5
        [HttpGet("/Brackets/Details/{id}/")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bracket = await _context.Brackets
                .FirstOrDefaultAsync(m => m.id == id);
            if (bracket == null)
            {
                return NotFound();
            }

            return View(bracket);
        }

        // GET: Brackets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Brackets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,BracketName,CreatedAt,TotalRounds")] Bracket bracket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bracket);
                await _context.SaveChangesAsync();
                /*
                 * For loop for TotalRounds * 2
                 * Then Create into Team
                 * id, TeamName=Null, BracketID
                 * string query = "SELECT * FROM Department WHERE DepartmentID = @p0";
                 * var TempQueryData = _context.Teams.FromSql("SET IDENTITY_INSERT [dbo].[Team] ON INSERT INTO[dbo].[Team]([id], [TeamName], [BracketID]) VALUES('" + i + "', ' ', '" + tempID + "') SET IDENTITY_INSERT[dbo].[Team] OFF").ToList();
                    Debug.WriteLine("HI: " + i);
                 */

                /* 3 rounds
                 * 2^3 = 8 teams
                 * 1v8, 2v7, 3v6, 4v5
                 * 1/8 v. 2/7, 3/6 v. 4/5
                 * 1/8/2/7 v. 3/6/4/5
                
                */
                int roundtotal = Convert.ToInt32(bracket.TotalRounds);
                int teamlength = (int)Math.Pow(2, roundtotal);
                Debug.WriteLine("Team Total: " + teamlength);
                int firstroundgames = teamlength / 2;
                int gametotal = teamlength - 1;
                int remaininggames = gametotal - firstroundgames;
                string tempID = bracket.id.ToString();
                List<Team> teamList = new List<Team>();
                for (int i = 0; i < teamlength; i++)
                {
                    var initial = new Team(){
                        TeamName = " ",
                        BracketID = bracket.id,
                    };
                    teamList.Add(initial);

                    _context.Teams.Add(initial);
                }
                _context.SaveChanges();
                /* Initial Games */
                List<int> topHalf = new List<int>();
                List<int> tempbottomHalf = new List<int>();
                for (int i = 0; i < teamlength; i++)
                {
                    if (i < (bracket.TotalRounds / 2))
                    {
                        topHalf.Add(teamList[i].id);
                    }
                    else
                    {
                        tempbottomHalf.Add(teamList[i].id);
                    }
                }
                List<int> bottomHalf = new List<int>();
                for (int i = 0; i < bottomHalf.Count(); i++)
                {
                    bottomHalf.Add(tempbottomHalf[bottomHalf.Count() - i - 1]);
                }
                //Create Rounds
                //Bracket, RoundNumber
                List<Round> roundList = new List<Round>();
                List<Match> matchList = new List<Match>();
                for (int i = 0; i < roundtotal; i++)
                {
                    var initial = new Round()
                    {
                        RoundNumber = i + 1,
                        BracketID = bracket.id,
                    };
                    roundList.Add(initial);

                    _context.Rounds.Add(initial);
                    // First Round Matches
                    for (int x = 0; x < firstroundgames; x++)
                    {

                        try
                        {
                            var matchInitial = new Match()
                            {
                                //MatchNumber, TEAMAID, TEAMBID        
                                MatchNumber = x,
                                TeamAID = topHalf[x],
                                TeamBID = bottomHalf[x],
                            };
                            matchList.Add(matchInitial);
                            _context.Matches.Add(matchInitial);
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            Debug.WriteLine("Out of Range Detected");
                        }

                    }
                   

                }
                _context.SaveChanges();
                

          
                await _context.SaveChangesAsync();
                
                return RedirectToAction(nameof(CreatePlayer), new { id = bracket.id });
            }
            return View(bracket);
        }

        // GET: Brackets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bracket = await _context.Brackets.FindAsync(id);
            if (bracket == null)
            {
                return NotFound();
            }
            return View(bracket);
        }

        // POST: Brackets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,BracketName,CreatedAt,TotalRounds")] Bracket bracket)
        {
            if (id != bracket.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bracket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BracketExists(bracket.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        // Create Players
        [HttpGet("/Brackets/{id}/ParticipantAdd")]
        public async Task<IActionResult> CreatePlayer(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bracket = await _context.Brackets
                .FirstOrDefaultAsync(m => m.id == id);
            if (bracket == null)
            {
                return NotFound();
            }

            return View(bracket);
        }
        [HttpPost("/Brackets/{id}/ParticipantAdd")]
        public ActionResult Teamroll(int id, string[] TeamTemp, [Bind("id,BracketName,CreatedAt,TotalRounds")] Bracket bracket)
        {
            for (int i = 0; i < TeamTemp.Length; i++)
            {
                
                Debug.WriteLine("Team: " + TeamTemp[i] + "," + "TeamID:" + i.ToString());
                Debug.WriteLine("ID: " + id);
                
            }
            return RedirectToAction(nameof(Details), new { id = bracket.id });
        }

        //id, TeamName, BracketID
        //[HttpPost("/Brackets/{id}/ParticipantAdd")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePlayerPost(int id, string[] TeamTemp, [Bind("id,BracketName,CreatedAt,TotalRounds")] Bracket bracket)
        {
            var bracket1 = await _context.Brackets
                .FirstOrDefaultAsync(m => m.id == id);
            Debug.WriteLine("TotalRounds: " + bracket1.TotalRounds);
            Debug.WriteLine(TeamTemp);
            return RedirectToAction(nameof(Details), new { id = bracket.id });
        }
        // GET: Brackets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bracket = await _context.Brackets
                .FirstOrDefaultAsync(m => m.id == id);
            if (bracket == null)
            {
                return NotFound();
            }

            return View(bracket);
        }

        // POST: Brackets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bracket = await _context.Brackets.FindAsync(id);
            _context.Brackets.Remove(bracket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BracketExists(int id)
        {
            return _context.Brackets.Any(e => e.id == id);
        }


    }
}
