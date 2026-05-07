using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TMS_API.DAL;
using TMS_SharedLibrary.Models;
using TMS_API.Models.DTO;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherRepo _teacherRepo;
        private readonly IMapper _mapper;

        public TeachersController(ITeacherRepo teacherRepo, IMapper mapper)
        {
            _teacherRepo = teacherRepo;
            _mapper = mapper;
        }

        // GET: api/<TeachersController>
        [HttpGet]
        public ActionResult<IEnumerable<TeacherDTO>> Get()
        {
            List<Teacher> teachers = _teacherRepo.GetAll();
            IEnumerable<TeacherDTO> result = _mapper.Map<IEnumerable<TeacherDTO>>(teachers);
            return Ok(result);
        }

        // GET api/<TeachersController>/5
        [HttpGet("{id}")]
        public ActionResult<TeacherDTO> Get(int id)
        {
            var teacher = _teacherRepo.FindById(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<TeacherDTO>(teacher));
        }

        // POST api/<TeachersController>
        [HttpPost]
        public ActionResult<TeacherDTO> Post([FromBody] TeacherDTO teacherDto)
        {
            if (teacherDto == null)
            {
                return BadRequest("Teacher data is required");
            }

            var teacher = _mapper.Map<Teacher>(teacherDto);
            var createdTeacher = _teacherRepo.Create(teacher);
            
            return CreatedAtAction(nameof(Get), new { id = createdTeacher.TeacherId }, _mapper.Map<TeacherDTO>(createdTeacher));
        }
    }
}
