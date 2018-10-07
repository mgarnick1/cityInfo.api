﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Controllers
{
	[Route("api/cities")]
	public class PointsOfInterestController : Controller
	{
		[HttpGet("{cityId}/pointsofinterest")]
		public IActionResult GetPointsOfInterest(int cityId)
		{
			var city = CityDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

			if (city == null)
			{
				return NotFound();
			}
			return Ok(city);
		}

		[HttpGet("{cityId}/pointsofinterest/{id}")]
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
	}
}
