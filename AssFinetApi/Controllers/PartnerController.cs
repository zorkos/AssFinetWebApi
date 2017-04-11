using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.Http;
using AssFinetApi.Models;

namespace AssFinetApi.Controllers
{
    public class PartnerController : ApiController
    {

        [HttpPost]
        public IHttpActionResult PostPersonalDetails( [FromBody] Partner personaldetails ) {
            if( !ModelState.IsValid ) 
                return BadRequest( ModelState );

            //db.PersonalDetails.Add( personaldetails );


            //return CreatedAtRoute( "DefaultApi", new { id = personaldetails.AutoId }, personaldetails );
            return null;
        }

    }
}
