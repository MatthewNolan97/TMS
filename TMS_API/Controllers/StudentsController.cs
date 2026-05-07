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
    public class StudentsController : ControllerBase
    {
        private readonly IStudentRepo _studentRepo;
        private readonly IMapper _mapper;
        private readonly IToyRepo _toyRepo;
        public StudentsController(IStudentRepo studentRepo, IMapper mapper, IToyRepo toyRepo)
        {
            _studentRepo = studentRepo;
            _mapper = mapper;
            _toyRepo = toyRepo;
        }

        // GET: api/<StudentsController>
        [HttpGet]
        public ActionResult<IEnumerable<StudentDTO>> Get()
        {
            List<Student> students = _studentRepo.GetAll();
            IEnumerable<StudentDTO> result = _mapper.Map<IEnumerable<StudentDTO>>(students);
            return Ok(result);
        }

        // GET api/<StudentsController>/5
        [HttpGet("{id}")]
        public ActionResult<StudentDTO> Get(int id)
        {
          var student = _studentRepo.FindById(id);
            if(student == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<StudentDTO>(student));
        }
        [HttpGet("ByOID/{id}")]
        public ActionResult<int> GetByOID(string id)
        {
            var student = _studentRepo.FindByOid(id);
            if (student == null)
            {
                return NotFound();
            }
            return Ok(student);
        }

        [HttpPut("BorrowToy/{id}")]
        public IActionResult Update(int id, int toyId)
        {
            var student = _studentRepo.FindById(id);
            var toy = _toyRepo.FindById(toyId);
            if (student == null)
            {
                return NotFound();
            }
            if(toy ==null)
            {
                return BadRequest("Toy not found");
            }
            if((bool)!toy.IsAvailable)
            {
                return BadRequest("Toy is not available");
            }
            _studentRepo.BookToy(id, toyId);
            return NoContent();
        }
        [HttpPut("ReturnToy/{id}")]
        public IActionResult ReturnToy(int id, int toyId)
        {
            var student = _studentRepo.FindById(id);
            var toy = _toyRepo.FindById(toyId);
            if (student == null)
            {
                return NotFound();
            }
            if (toy == null)
            {
                return BadRequest("Toy not found");
            }
            var valid = _studentRepo.checkToyIsBorrowedByStudent(id, toyId);
            if (!valid)
            {
                return BadRequest("Could not find a current toy loan for this student and toy");
            }
            _studentRepo.ReturnToy(id, toyId);
            return NoContent();
        }

        // GET api/Students/5/BorrowHistory
        [HttpGet("{id}/BorrowHistory")]
        public IActionResult GetBorrowHistory(int id)
        {
            var student = _studentRepo.FindById(id);
            if (student == null)
            {
                return NotFound("Student not found");
            }

            var borrowHistory = _studentRepo.GetBorrowHistory(id);
            var borrowHistoryDTO = _mapper.Map<List<ToyLoanDTO>>(borrowHistory);
            return Ok(borrowHistoryDTO);
        }
        // GET api/Students/5/CurrentLoans
        [HttpGet("{id}/CurrentLoans")]
        public IActionResult GetCurrentLoans(int id)
        {
            var student = _studentRepo.FindById(id);
            if (student == null)
            {
                return NotFound("Student not found");
            }
            var borrowHistory = _studentRepo.GetBorrowHistory(id)
                .Where(tl => tl.ReturnDate == null)
                .ToList();
            var borrowHistoryDTO = _mapper.Map<List<ToyLoanDTO>>(borrowHistory);
            return Ok(borrowHistoryDTO);
        }
    }
}
