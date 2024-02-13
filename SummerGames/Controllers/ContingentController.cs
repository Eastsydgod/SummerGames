using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SummerGames.Data;
using SummerGames.Models;

namespace SummerGames.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContingentController : ControllerBase
    {
        private readonly SummerGamesContext _context;

        public ContingentController(SummerGamesContext context)
        {
            _context = context;
        }

        // GET: api/Contingent
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContingentDTO>>> GetContingents()
        {
            return await _context.Contingents
                .Include(s => s.Athletes)
                .Select(s => new ContingentDTO
                {
                    ID = s.ID,
                    Code = s.Code,
                    Name = s.Name
                }).ToListAsync();


        }

        // GET: api/Contingent/ inc - Include the Athletes
        [HttpGet("inc/{id}")]
        public async Task<ActionResult<ContingentDTO>> GetContingent(int id)
        {
            var contingentDTO = await _context.Contingents
                 .Include(s => s.Athletes)
                 .Select(s => new ContingentDTO
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

            if (contingentDTO == null)
            {
                return NotFound(new { message = "Error: Contingent record not found, Please try again. If the issue persists, contact you Administrator." });
            }

            return contingentDTO;
        }



        // GET: api/Sport/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ContingentDTO>> GetSport(int id)
        {
            var contingentDTO = await _context.Contingents
                .Select(s => new ContingentDTO
                {
                    ID = s.ID,
                    Code = s.Code,
                    Name = s.Name
                })
                .FirstOrDefaultAsync(p => p.ID == id);

            if (contingentDTO == null)
            {
                return NotFound(new { message = "Error: Contingent record not found, Please try again. If the issue persists, contact you Administrator." });
            }

            return contingentDTO;

        }


        // PUT: api/Contingent/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContingent(int id, ContingentDTO contingentDTO)
        {
            if (id != contingentDTO.ID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var contingentToUpdate = await _context.Contingents.FindAsync(id);


            if (contingentToUpdate == null)
            {
                return NotFound(new { message = "Error: Contingent record not found." });
            }


            contingentToUpdate.ID = contingentDTO.ID;
            contingentToUpdate.Code = contingentDTO.Code;
            contingentToUpdate.Name = contingentDTO.Name;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContingentExists(id))
                {
                    return Conflict(new { message = "Concurrency Error: Contingent has been Removed." });
                }
                else
                {
                    return Conflict(new { message = "Concurrency Error: Contingent has been updated by another user.  Back out and try editing the record again." });
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE"))
                {
                    return BadRequest(new { message = "Unable to save: Duplicate Contingent Code number." });
                }
                else
                {
                    return BadRequest(new { message = "Unable to save changes to the database. Try again, and if the problem persists see your system administrator." });
                }
            }


        }

        // POST: api/Contingent
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ContingentDTO>> PostContingent(ContingentDTO contingentDTO)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Contingent contingent = new Contingent
            {
                ID = contingentDTO.ID,
                Code = contingentDTO.Code,
                Name = contingentDTO.Name
            };

            try
            {
                _context.Contingents.Add(contingent);
                await _context.SaveChangesAsync();

                //Assign Database Generated values back into the DTO
                contingentDTO.ID = contingent.ID;
                

                return CreatedAtAction(nameof(GetSport), new { id = contingent.ID }, contingentDTO);
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE"))
                {
                    return BadRequest(new { message = "Unable to save: Duplicate Contingent Code number." });
                }
                else
                {
                    return BadRequest(new { message = "Unable to save changes to the database. Try again, and if the problem persists see your system administrator." });
                }
            }
        }

        // DELETE: api/Contingent/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContingent(int id)
        {
            var contingent = await _context.Contingents.FindAsync(id);
            if (contingent == null)
            {
                return NotFound();
            }
            try
            {
                _context.Contingents.Remove(contingent);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    return BadRequest(new { message = "Delete Error: Remember, you cannot delete a Contingent that has Athletes assigned." });
                }
                else
                {
                    return BadRequest(new { message = "Delete Error: Unable to delete Sport. Try again, and if the problem persists see your system administrator." });
                }
            }
        }

        private bool ContingentExists(int id)
        {
            return _context.Contingents.Any(e => e.ID == id);
        }
    }
}
