using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using TurnupAPI.Controllers;
using TurnupAPI.Forms;
using TurnupAPI.Interfaces;
using TurnupAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using TurnupAPI.Data;
using TurnupAPI.DTO;
using AutoMapper;
using TurnupAPI.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TurnupTest
{
    [TestClass]
    public class AdminControllerTests
    {
        private static  AdminController CreateController(TurnupContext context, Mock<IArtistRepository> mockArtistRepository )
        {
            var mockLogger = new Mock<ILogger<AdminController>>();
            var mockMapper = new Mock<IMapper>();

            return new AdminController(
                null, null,
                mockArtistRepository.Object, context, null, null, null, mockLogger.Object, mockMapper.Object, null);
        }

        [TestMethod]
        public async Task AddArtist_ArtistExists_ReturnsConflict()
        {
            using var context = new TurnupContext(new DbContextOptionsBuilder<TurnupContext>().UseInMemoryDatabase(databaseName: "InMemoryDatabase").Options);
            var existingArtist = new Artist
            {
                Name = "test name2",
                Country = "Test country",
                Description = "Description",
                Picture = "test picture"
            };

            context.Artist.Add(existingArtist);
            context.SaveChanges();

            var artistForm = new ArtistForm
            {
                Name = "test name2",
                Country = "Test country",
                Description = "Description",
                Picture = "test picture"
            };

            var mockArtistRepository = new Mock<IArtistRepository>();
            mockArtistRepository.Setup(repo => repo.ArtistExistsAsync(artistForm)).ReturnsAsync(true);

            var controller = CreateController(context, mockArtistRepository);

            var result = await controller.AddArtist(artistForm);

            Assert.IsInstanceOfType(result, typeof(ConflictResult));
        }
        [TestMethod]
        public async Task AddArtist_ArtistNotExists_ReturnsNoContent()
        {
            using var context = new TurnupContext(new DbContextOptionsBuilder<TurnupContext>().UseInMemoryDatabase(databaseName: "InMemoryDatabase").Options);
            var artistForm = new ArtistForm
            {
                Name = "test name2",
                Country = "Test country",
                Description = "Description",
                Picture = "test picture"
            };

            var mockArtistRepository = new Mock<IArtistRepository>();
            mockArtistRepository.Setup(repo => repo.ArtistExistsAsync(artistForm)).ReturnsAsync(false);

            var controller = CreateController(context, mockArtistRepository);

            var result = await controller.AddArtist(artistForm);
            /*
            var artistInDatabase = context.Artist.FirstOrDefault(a => a.Name == "test name2");
            Assert.IsNotNull(artistInDatabase);
            // Assert.AreEqual("Test country", artistInDatabase.Country);
            */
            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }
        [TestMethod]
        public async Task UpdateArtist_ArtistIdNotEqualsId_ReturnsBadRequest()
        {
            using var context = new TurnupContext(new DbContextOptionsBuilder<TurnupContext>().UseInMemoryDatabase(databaseName: "InMemoryDatabase2").Options);
            int id = 1;
            var artist = new Artist()
            {
                Id = 2,
                Name = "test",
                Description = "description",
                Country =" country",
                Picture = "picture"
            };
           

            var mockArtistRepository = new Mock<IArtistRepository>();
           // mockArtistRepository.Setup(repo => repo.ArtistExistsAsync(artistForm)).ReturnsAsync(false);
            var controller = CreateController(context, mockArtistRepository);
            var result = await controller.UpdateArtist(id, artist);
           
            var statusCodeResult = (StatusCodeResult)result;
            Assert.AreEqual(400, statusCodeResult.StatusCode);
           
        }

        [TestMethod]
        public async Task UpdateArtist_ArtistIdEqualsId_Ok()
        {
            using var context = new TurnupContext(new DbContextOptionsBuilder<TurnupContext>().UseInMemoryDatabase(databaseName: "InMemoryDatabase2").Options);
            var artist = new Artist()
            {
                Id = 2,
                Name = "test",
                Description = "description",
                Country = " country",
                Picture = "picture"
            };
            context.Artist.Add(artist);
            context.SaveChanges();
            int id = 2;
            var artistToUpdate = new Artist()
            {
                Id = 2,
                Name = "test",
                Description = "description",
                Country = " country",
                Picture = "picture"
            };


            var mockArtistRepository = new Mock<IArtistRepository>();
             mockArtistRepository.Setup(repo => repo.UpdateAsync(artistToUpdate)).ReturnsAsync(true);

            var controller = CreateController(context, mockArtistRepository);

            var result = await controller.UpdateArtist(id, artistToUpdate);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var content = (OkObjectResult)result;
            var value = content.Value;
            var message = value?.ToString();
            Assert.AreEqual("L'artiste a été modifié avec succès", message);
        }

        [TestMethod]
        public async Task UpdateArtist_ArtistIdEqualsIdButArtistNotExists_returnsOk()
        {
            using var context = new TurnupContext(new DbContextOptionsBuilder<TurnupContext>().UseInMemoryDatabase(databaseName: "InMemoryDatabase2").Options);
            var artist = new Artist()
            {
                Id = 6,
                Name = "test",
                Description = "description",
                Country = " country",
                Picture = "picture"
            };
            context.Artist.Add(artist);
            context.SaveChanges();
            int id = 3;
            var artistToUpdate = new Artist()
            {
                Id = 3,
                Name = "test",
                Description = "description",
                Country = " country",
                Picture = "picture"
            };


            var mockArtistRepository = new Mock<IArtistRepository>();
            mockArtistRepository.Setup(repo => repo.UpdateAsync(artistToUpdate)).ReturnsAsync(false);

            var controller = CreateController(context, mockArtistRepository);

            var result = await controller.UpdateArtist(id, artistToUpdate);


            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var content = (OkObjectResult)result;
            var value = content.Value;
            var message = value?.ToString();
            Assert.AreEqual("L'artiste n'a pas pu etre modifié.", message);
        }
    }
}
