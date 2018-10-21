using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Cors;
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
	[EnableCors("CorsPolicy")]
	public class PointsOfInterestController : Controller
	{

		private ILogger<PointsOfInterestController> _logger;
		private IMailService _mailService;
		private ICityInfoRepository _cityInfoRepository;

		public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, ICityInfoRepository cityInfoRepository)
		{
			_logger = logger;
			_mailService = mailService;
			_cityInfoRepository = cityInfoRepository;
			//HttpContext.RequestServices.GetService();
		}

		[HttpGet("{cityId}/pointsofinterest")]
		public IActionResult GetPointsOfInterest(int cityId)
		{
			try 
			{
				if (!_cityInfoRepository.CityExists(cityId))
				{
					_logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
					return NotFound();
				}

				var pointsOfInterestForCity = _cityInfoRepository.GetPointsOfInterestForCity(cityId);

				//var pointsOfInterestForCityResults = new List<PointOfInterestDto>();
				//foreach (var poi in pointsOfInterestForCity)
				//{
				//	pointsOfInterestForCityResults.Add(new PointOfInterestDto()
				//	{
				//		Id = poi.Id,
				//		Name = poi.Name,
				//		Description = poi.Description
				//	});
				//}
				var pointsOfInterestForCityResults = 
					Mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity);

				return Ok(pointsOfInterestForCityResults);

				//var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

				//if (city == null)
				//{
				//	_logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest.");
				//	return NotFound();
				//}
				//return Ok(city.PointOfInterest);
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

			if (!_cityInfoRepository.CityExists(cityId))
			{
				return NotFound();
			}

			var pointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

			if (pointOfInterest == null)
			{
				return NotFound();
			}

			//var pointOfInterestResult = new PointOfInterestDto()
			//{
			//	Id = pointOfInterest.Id,
			//	Name = pointOfInterest.Name,
			//	Description = pointOfInterest.Description
			//};
			var pointOfInterestResult = Mapper.Map<PointOfInterestDto>(pointOfInterest);
			return Ok(pointOfInterestResult);


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

			if (!_cityInfoRepository.CityExists(cityId))
			{
				return NotFound();
			}

			var finalPointOfInterest = Mapper.Map<Entities.PointOfInterest>(pointOfInterest);
			_cityInfoRepository.AddPointOfInterestForCity(cityId, finalPointOfInterest);
		
			if (!_cityInfoRepository.Save())
			{
				return StatusCode(500, "A problem happened while handling your request.");
			}


			var createdPointOfInterestToReturn = Mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);
			return CreatedAtRoute("GetPointOfInterest", new 
			{ cityId = cityId, id = createdPointOfInterestToReturn.Id}, createdPointOfInterestToReturn);
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

			//var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			//if (city == null)
			//{
			//	return NotFound();
			//}

			if (!_cityInfoRepository.CityExists(cityId))
			{
				return NotFound();
			}


			//var pointsOfInterestFromStore = city.PointOfInterest.FirstOrDefault(p => p.Id == id);

			//if (pointsOfInterestFromStore == null)
			//{
			//	return NotFound();
			//}
			var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
			if (pointOfInterestEntity == null)
			{
				return NotFound();
			}

			Mapper.Map(pointOfInterest, pointOfInterestEntity);
			
			if (!_cityInfoRepository.Save())
			{
				return StatusCode(500, "a problem happened while handling your request.");
			}
			//pointsOfInterestFromStore.Name = pointOfInterest.Name;
			//pointsOfInterestFromStore.Description = pointOfInterest.Description;

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

			//var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			//if (city == null)
			//{
			//	return NotFound();
			//}

			if (!_cityInfoRepository.CityExists(cityId))
			{
				return NotFound();
			}
			//var pointsOfInterestFromStore = city.PointOfInterest.FirstOrDefault(p => p.Id == id);

			//if (pointsOfInterestFromStore == null)
			//{
			//	return NotFound();
			//}
			var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
			if (pointOfInterestEntity == null)
			{
				return NotFound();
			}

			var pointOfInterestToPatch = Mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);
			
			//var pointOfInterestToPatch = 
			//	new PointOfInterestForUpdateDto()
			//	{
			//		Name = pointsOfInterestFromStore.Name,
			//		Description = pointsOfInterestFromStore.Description
			//	};
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

			Mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);
			//pointsOfInterestFromStore.Name = pointOfInterestToPatch.Name;
			//pointsOfInterestFromStore.Description = pointOfInterestToPatch.Description;
			if (!_cityInfoRepository.Save())
			{
				return StatusCode(500, "A problem happened while handling your request.");
			}
			return NoContent();
		}

		[HttpDelete("{cityId}/pointsofinterest/{id}")]
		public IActionResult DeletePointOfInterest(int cityId, int id)
		{

			if (!_cityInfoRepository.CityExists(cityId))
			{
				return NotFound();
			}

			var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
			if (pointOfInterestEntity == null)
			{
				return NotFound();
			}

			_cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);

			if (!_cityInfoRepository.Save())
			{
				return StatusCode(500, "A problem happened while handling your request.");
			}

			_mailService.Send("Point of interest deleted.",
					$"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

			return NoContent();
		}
	}
}
