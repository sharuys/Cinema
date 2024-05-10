﻿using Cinema.DAL.Models;
using Cinema.DAL;
using Cinema.DAL.Implemantations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Cinema.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Roles = $"{SD.Admin},{SD.Customer}")]
    public class MovieController : ControllerBase
    {
        private readonly ILogger<MovieController> _logger;
        private readonly UnitOfWork _unitOfWork;

        public MovieController(ILogger<MovieController> logger,
            UnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetMovies")]
        public async Task<ActionResult<List<Movie>>> GetMoviesAsync() => Ok(await _unitOfWork.MovieRepository.Get());


        [HttpPost("AddMovie")]
        public async Task<IActionResult> AddMovieAsync([FromBody] Movie movie)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _unitOfWork.MovieRepository.InsertAsync(movie);
            await _unitOfWork.SaveAsync();
            return Ok();
        }


        [HttpDelete("DeleteMovie/{id}")]
        public async Task<IActionResult> DeleteMovieAsync(int id)
        {
            var movie = await _unitOfWork.MovieRepository.GetByIDAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            await _unitOfWork.MovieRepository.DeleteAsync(id);
            return Ok();
        }

        [HttpPut("UpdateMovie/{id}")]
        public async Task<IActionResult> UpdateMovieAsync(int id, [FromBody] Movie updatedMovie)
        {
            if (id != updatedMovie.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _unitOfWork.MovieRepository.UpdateAsync(id, updatedMovie);
                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _unitOfWork.MovieRepository.GetByIDAsync(id) == null)
                {
                    return NotFound();
                }
                throw;
            }
        }




        [HttpGet("GetActualMovies")]
        public async Task<ActionResult<List<Movie>>> GetActualMoviesAsync()
        {
            return Ok(await _unitOfWork.MovieRepository.Get(m => m.NowShowing));
        }


        [HttpGet("GetLatestMovies")]
        public async Task<ActionResult<List<Movie>>> GetLatestMoviesAsync()
        {
            var currentDate = DateTime.Now;
            var latestDate = currentDate.AddMonths(-1);
            return Ok(await _unitOfWork.MovieRepository.Get(m => m.ReleaseDate >= latestDate));
        }



    }
}
