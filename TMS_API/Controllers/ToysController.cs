using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TMS_API.DAL;
using TMS_API.Models.DTO;
using TMS_SharedLibrary.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class ToysController : ControllerBase
    {
        private readonly IToyRepo _repo;
        private readonly IMapper _mapper;

        public ToysController(IToyRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<ToyDTO>> Get()
        {
            List<Toy> toys = _repo.GetAll();
            IEnumerable<ToyDTO> result = _mapper.Map<IEnumerable<ToyDTO>>(toys);
            return Ok(result);
        }
        
        [HttpGet("filter")]
        public ActionResult<IEnumerable<ToyDTO>> GetFiltered(
            [FromQuery] string[]? materials,
            [FromQuery] string[]? categories)
        {
            List<Toy> toys;
            
            if ((materials == null || !materials.Any()) && 
                (categories == null || !categories.Any()))
            {
                toys = _repo.GetAll();
            }
            else
            {
                toys = _repo.GetFiltered(materials, categories);
            }
            
            var result = _mapper.Map<IEnumerable<ToyDTO>>(toys);
            return Ok(result);
        }

        // GET api/<ToysController>/5
        [HttpGet("{id}")]
        public ActionResult<ToyDTO> Get(int id)
        {
            var toy = _repo.FindById(id);
            if (toy == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<ToyDTO>(toy));
        }

        // POST api/<ToysController>
        [HttpPost]
        public IActionResult Create(ToyDTO toyDTO)
        {
            var toy = _mapper.Map<Toy>(toyDTO);
            toy.IsActive = true;
            _repo.Add(toy);
            return CreatedAtAction(nameof(Create), new { id = toy.ToyId }, toy);
        }

        // PUT api/<ToysController>/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, ToyDTO toyDTO)
        {
            if (id != toyDTO.ToyId)
            {
                return BadRequest();
            }
            var existingToy = _repo.FindById(id);
            if (existingToy == null)
            {
                return NotFound();
            }
            var toy = _mapper.Map<Toy>(toyDTO);
            _repo.Update(id, toy);
            return NoContent();
        }

        // DELETE api/<ToysController>/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var toy = _repo.FindById(id);
            if (toy == null)
            {
                return NotFound();
            }

            if (toy.IsAvailable == false)
                return BadRequest("Cannot delete this toy because it is currently borrowed.");

            _repo.Delete(id);
            return NoContent();
        }
    }
}
