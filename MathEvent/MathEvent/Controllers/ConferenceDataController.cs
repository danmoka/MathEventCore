using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MathEvent.Helpers;
using MathEvent.Models;
using MathEvent.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MathEvent.Controllers
{
    [Route("api/conferences")]
    [ApiController]
    public class ConferenceDataController : ControllerBase
    {
        private readonly ApplicationContext _db;

        public ConferenceDataController(ApplicationContext db)
        {
            _db = db;
        }

        [HttpPost]
        [Route("delete")]
        public async Task<HttpStatusCode> DeleteConference(
            [Bind("Id", "UserId", "UserRoles")] ConferenceViewModel conferenceModel)
        {
            if (!await IsConferenceModifier(conferenceModel.Id, conferenceModel.UserId, conferenceModel.UserRoles))
            {
                return HttpStatusCode.Forbidden;
            }

            var conference = await _db.Conferences.Where(c => c.Id == conferenceModel.Id).SingleOrDefaultAsync();

            if (conference == null)
            {
                return HttpStatusCode.NotFound;
            }

            var path = UserDataPathWorker.GetRootPath(conference.DataPath);

            _db.Conferences.Remove(conference);
            await _db.SaveChangesAsync();

            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch
                {
                    return HttpStatusCode.InternalServerError;
                }

            }

            return HttpStatusCode.OK;
        }

        [HttpPost]
        [Route("edit")]
        public async Task<HttpStatusCode> EditConference(
            [Bind("Id", "Name", "Location", "Start", "End", "UserId", "UserRoles", "SectionViewModels")] ConferenceViewModel conferenceModel)
        {
            if (!await IsConferenceModifier(conferenceModel.Id, conferenceModel.UserId, conferenceModel.UserRoles))
            {
                return HttpStatusCode.Forbidden;
            }

            var conference = await _db.Conferences.Where(c => c.Id == conferenceModel.Id).SingleOrDefaultAsync();

            if (conference == null)
            {
                return HttpStatusCode.NotFound;
            }

            // нужна сущность-переводчик для перевода из модели в модель
            conference.Name = conferenceModel.Name;
            conference.Location = conferenceModel.Location;
            conference.Start = conferenceModel.Start;
            conference.End = conferenceModel.End;

            _db.Conferences.Update(conference);
            await _db.SaveChangesAsync();

            return HttpStatusCode.OK;
        }

        private async Task<bool> IsConferenceModifier(int conferenceId, string userId, List<string> userRoles)
        {
            var conference = await _db.Conferences.Where(c => c.Id == conferenceId).SingleOrDefaultAsync();

            var isModifier = false;

            if (conference == null)
            {
                return isModifier;
            }

            if (conference.ManagerId == userId)
            {
                isModifier |= true;
            }

            foreach (var userRole in userRoles)
            {
                if (userRole == "admin")
                {
                    isModifier |= true;
                    break;
                }
            }

            return isModifier;
        }
    }
}