using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
	[Route("api/cities")]
	public class PointsOfInterestController : Controller
	{

		private ILogger<PointsOfInterestController> _logger;
		private IMailService _mailService;

		public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService)
		{
			_logger = logger;
			_mailService = mailService;
			//HttpContext.RequestServices.GetService();
		}

		[HttpGet("{cityId}/pointsofinterest")]
		public IActionResult GetPointsOfInterest(int cityId)
		{
			try 
			{
				var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

				if (city == null)
				{
					_logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
					return NotFound();
				}
				return Ok(city.PointOfInterest);
			}
			catch (Exception ex)
			{
				_logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.", ex);
				return StatusCode(500, "A problem happened while handling your request.");
			}
		}

		[HttpGet("{cityId}/pointsofinterest/{id}", Name = "GetPointOfInterest")]
		public IActionResult GetPointOfInterest(int cityId, int id)
		{
			var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			var pointOfInterest = city.PointOfInterest.FirstOrDefault(p => p.Id == id);

			if(pointOfInterest == null)
			{
				return NotFound();
			}
			return Ok(pointOfInterest);
		}
		[HttpPost("{cityId}/pointsofinterest")]
		public IActionResult CreatePointOfInterest(int cityId, 
			[FromBody] PointOfInterestForCreationDto pointOfInterest)
		{
			if (pointOfInterest == null)
			{
				return BadRequest();
			}

			if(pointOfInterest.Description == pointOfInterest.Name)
			{
				ModelState.AddModelError("Description", "The provided description should be different from the name.");
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}
			var maxPointOfInterestId = CityDataStore.Current.Cities.SelectMany(
				c => c.PointOfInterest).Max(p => p.Id);
			var finalPointOfInterest = new PointOfInterestDto()
			{
				Id = ++maxPointOfInterestId,
				Name = pointOfInterest.Name,
				Description = pointOfInterest.Description
			};
			city.PointOfInterest.Add(finalPointOfInterest);

			return CreatedAtRoute("GetPointOfInterest", new { cityId = cityId, id = finalPointOfInterest.Id}, finalPointOfInterest);
		}
		[HttpPut("{cityId}/pointsofinterest/{id}")]
		public IActionResult UpdatePointOfInterest(int cityId, int id, 
			[FromBody] PointOfInterestForUpdateDto pointOfInterest)
		{
			if (pointOfInterest == null)
			{
				return BadRequest();
			}
			if (pointOfInterest.Description == pointOfInterest.Name)
			{
				ModelState.AddModelError("Description", "The provided description should be different from the name.");
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			var pointsOfInterestFromStore = city.PointOfInterest.FirstOrDefault(p => p.Id == id);
			
			if (pointsOfInterestFromStore == null)
			{
				return NotFound();
			}

			pointsOfInterestFromStore.Name = pointOfInterest.Name;
			pointsOfInterestFromStore.Description = pointOfInterest.Description;

			return NoContent();
			
		}
		[HttpPatch("{cityId}/pointsofinterest/{Id}")]
		public IActionResult PartiallyUpdatePointsOfInterest(int cityId, int id,
			[FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
		{
			if (patchDoc == null)
			{
				return BadRequest();
			}

			var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}
			var pointsOfInterestFromStore = city.PointOfInterest.FirstOrDefault(p => p.Id == id);

			if (pointsOfInterestFromStore == null)
			{
				return NotFound();
			}

			var pointOfInterestToPatch = 
				new PointOfInterestForUpdateDto()
				{
					Name = pointsOfInterestFromStore.Name,
					Description = pointsOfInterestFromStore.Description
				};
			patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);
			
			if(!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			
			if (pointOfInterestToPatch.Description == pointOfInterestToPatch.Name)
			{
				ModelState.AddModelError("Description", "The provided description should be different from the name.");
			}

			TryValidateModel(pointOfInterestToPatch);
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			pointsOfInterestFromStore.Name = pointOfInterestToPatch.Name;
			pointsOfInterestFromStore.Description = pointOfInterestToPatch.Description;

			return NoContent();
		}
		[HttpDelete("{cityId}/pointsofinterest/{id}")]
		public IActionResult DeletePointsOfInterest(int cityId, int id)
		{
			var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}

			var pointsOfInterestFromStore = city.PointOfInterest.FirstOrDefault(p => p.Id == id);

			if (pointsOfInterestFromStore == null)
			{
				return NotFound();
			}

			city.PointOfInterest.Remove(pointsOfInterestFromStore);

			_mailService.Send("Point of Interest deleted.", $"Point of Interest {pointsOfInterestFromStore.Name} with id {pointsOfInterestFromStore.Id} was deleted.");

			return NoContent();
		}
	}
}
