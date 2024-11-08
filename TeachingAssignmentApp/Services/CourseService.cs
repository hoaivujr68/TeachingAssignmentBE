using AutoMapper;
using TeachingAssignmentApp.Data;
using TeachingAssignmentApp.Model;
using TeachingAssignmentApp.Repositories;

namespace TeachingAssignmentApp.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;

        public CourseService(ICourseRepository courseRepository, IMapper mapper)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
        }

        public async Task<Pagination<Course>> GetAllAsync(CourseQueryModel queryModel)
        {
            return await _courseRepository.GetAllAsync(queryModel);
        }

        public async Task<CourseModel> GetByIdAsync(Guid id)
        {
            var course = await _courseRepository.GetByIdAsync(id);
            return _mapper.Map<CourseModel>(course);
        }

        public async Task AddAsync(CourseModel courseModel)
        {
            var newCourse = _mapper.Map<Course>(courseModel);
            await _courseRepository.AddAsync(newCourse);
        }

        public async Task UpdateAsync(CourseModel courseModel)
        {
            var updateCourse = _mapper.Map<Course>(courseModel);
            await _courseRepository.UpdateAsync(updateCourse);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _courseRepository.DeleteAsync(id);
        }
    }
}
