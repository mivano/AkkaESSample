using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using AkkaES.Business.Customers;
using AkkaES.Core.EventSourcing;
using Microsoft.AspNetCore.Mvc;

namespace AkkaES.Web.Controllers
{
    [Route("api/[controller]")]
    [FormatFilter]
    public class CustomersController : Controller
    {
        private readonly ActorSystem _actorSystem;

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);

        public CustomersController(ActorSystem actorSystem)
        {
            _actorSystem = actorSystem;
        }

        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return NotFound();
            // _actorSystem.ActorSelection("CustomerCoordinator")
        }

        // GET api/values/5
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var item = await _actorSystem.ActorSelection("/user/CustomerCoordinator").Ask<CustomerEntity>(new GetState(id), DefaultTimeout).ConfigureAwait(false);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]CustomerEntity customer)
        {
            customer.Id = Guid.NewGuid();

            _actorSystem.ActorSelection("/user/CustomerCoordinator").Tell(new CreateCustomerCommand(customer.Id, customer.Name));

            return Accepted(Url.Action("Get", new { id = customer.Id }), customer);
        }

        // PUT api/values/5
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Put(Guid id, [FromBody]CustomerEntity customer)
        {
            var actor = _actorSystem.ActorSelection("/user/CustomerCoordinator");

            var item = await actor.Ask<CustomerEntity>(new GetState(id), DefaultTimeout).ConfigureAwait(false);

            if (item == null)
                return NotFound();

            actor.Tell(new UpdateCustomerCommand(customer.Id, customer.Name));

            return Accepted(Url.Action("Get", new { id = customer.Id }), customer);
        }

        // DELETE api/values/5
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var actor = _actorSystem.ActorSelection("/user/CustomerCoordinator");

            var item = await actor.Ask<CustomerEntity>(new GetState(id), DefaultTimeout).ConfigureAwait(false);

            if (item == null)
                return NotFound();

            actor.Tell(new RemoveCustomerCommand(id));

            return NoContent();
        }
    }
}
