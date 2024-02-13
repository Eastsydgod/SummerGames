using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SummerGames.Data;
using SummerGames.Models;

namespace SummerGames.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportController : ControllerBase
    {
        private readonly SummerGamesContext _context;

        public SportController(SummerGamesContext context)
        {
            _context = context;
        }

        // GET: api/Sport
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SportDTO>>> GetSports()
        {
            return await _context.Sports
                .Include(s => s.Athletes)
                .Select(s => new SportDTO
                {
                    ID = s.ID,
                    Code = s.Code,
                    Name = s.Name
                }).ToListAsync();
        }


        // GET: api/Sport/inc - Include the Athletes
        [HttpGet("inc/{id}")]
        public async Task<ActionResult<SportDTO>> GetSportInc(int id)
        {
            var sportDTO = await _context.Sports.
                Include(s => s.Athletes)
                .Select(s => new SportDTO
                {
                    ID = s.ID,
                    Code = s.Code,
                    Name = s.Name,
                    Athletes = s.Athletes.Select(sAthlete => new AthleteDTO
                    {
                        ID = sAthlete.ID,
                        FirstName = sAthlete.FirstName,
                        MiddleName = sAthlete.MiddleName,
                        LastName = sAthlete.LastName,
                        AthleteCode = sAthlete.AthleteCode,
                        DOB = sAthlete.DOB,
                        Height = sAthlete.Height,
                        Weight = sAthlete.Weight,
                        Affiliation = sAthlete.Affiliation,
                        MediaInfo = sAthlete.MediaInfo,
                        Gender = sAthlete.Gender,
                        SportID = sAthlete.SportID
                    }).ToList()
                })
                 .FirstOrDefaultAsync(a => a.ID == id);

            if (sportDTO == null)
            {
                return NotFound(new { message = "Error: Sport record not found, Please try again. If the issue persists, contact you Administrator." });
            }

            return sportDTO;
        }






        // GET: api/Sport/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SportDTO>> GetSport(int id)
        {
            var sportDTO = await _context.Sports
                .Select(s => new SportDTO
                {
                    ID = s.ID,
                    Code = s.Code,
                    Name = s.Name
                })
                .FirstOrDefaultAsync(p => p.ID == id);

            if (sportDTO == null)
            {
                return NotFound(new { message = "Error: Sport record not found, Please try again. If the issue persists, contact you Administrator." });
            }

            return sportDTO;

        }



        // PUT: api/Sport/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSport(int id, SportDTO sportDTO)
        {
            if (id != sportDTO.ID)
            {
                return BadRequest(new { message = "Error: ID does not match Sport" });
            }

           if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sportToUpdate = await _context.Sports.FindAsync(id);

            if(sportToUpdate == null)
            {
                return NotFound(new { message = "Error: Sport record not found." });
            }

            if (sportDTO.RowVersion != null)
            {
                if (!sportToUpdate.RowVersion.SequenceEqual(sportToUpdate.RowVersion))
                {
                    return Conflict(new { message = "Concurrency Error: Sport has been changed by another user.  Try editing the record again." });
                }
            }

       

                sportToUpdate.ID = sportDTO.ID;
                sportToUpdate.Code = sportDTO.Code;
                 sportToUpdate.Name = sportDTO.Name;

            //Put the original RowVersion value in the OriginalValues collection for the entity
            _context.Entry(sportToUpdate).Property("RowVersion").OriginalValue = sportDTO.RowVersion;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SportExists(id))
                {
                    return Conflict(new { message = "Concurrency Error: Sport has been Removed." });
                }
                else
                {
                    return Conflict(new { message = "Concurrency Error: Sport has been updated by another user.  Back out and try editing the record again." });
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE"))
                {
                    return BadRequest(new { message = "Unable to save: Duplicate Sport Code number." });
                }
                else
                {
                    return BadRequest(new { message = "Unable to save changes to the database. Try again, and if the problem persists see your system administrator." });
                }
            }

        }

        // POST: api/Sport
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SportDTO>> PostSport(SportDTO sportDTO)
        {

            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Sport sport = new Sport
            {
                ID = sportDTO.ID,
                Code = sportDTO.Code,
                Name = sportDTO.Name
            };

            try
            {
                _context.Sports.Add(sport);
                await _context.SaveChangesAsync();

                //Assign Database Generated values back into the DTO
                sportDTO.ID = sport.ID;
                sportDTO.RowVersion = sport.RowVersion;

                return CreatedAtAction(nameof(GetSport), new { id = sport.ID }, sportDTO);
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE"))
                {
                    return BadRequest(new { message = "Unable to save: Duplicate Sport Code number." });
                }
                else
                {
                    return BadRequest(new { message = "Unable to save changes to the database. Try again, and if the problem persists see your system administrator." });
                }
            }
        }

        // DELETE: api/Sport/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSport(int id)
        {
            var sport = await _context.Sports.FindAsync(id);
            if (sport == null)
            {
                return NotFound(new { message = "Delete Error: Doctor has already been removed." });
            }
            try
            {
                _context.Sports.Remove(sport);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    return BadRequest(new { message = "Delete Error: Remember, you cannot delete a Sport that has Athletes assigned." });
                }
                else
                {
                    return BadRequest(new { message = "Delete Error: Unable to delete Sport. Try again, and if the problem persists see your system administrator." });
                }
            }
        }
        private bool SportExists(int id)
        {
            return _context.Sports.Any(e => e.ID == id);
        }
    }
}
