using AutoMapper;
using FitnessClub.BLL.Dtos;
using FitnessClub.BLL.Interfaces;
using FitnessClub.Core.Abstractions;
using FitnessClub.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitnessClub.BLL.Services
{
    public class ClubService : IClubService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IRepository<Club> _clubRepository;

        public ClubService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _clubRepository = _unitOfWork.GetRepository<Club>();
        }

        public async Task<IEnumerable<ClubDto>> GetAllClubsAsync()
        {
            var clubs = await _clubRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ClubDto>>(clubs);
        }

        public async Task<ClubDto?> GetClubByIdAsync(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null)
            {
                return null;
            }
            return _mapper.Map<ClubDto>(club);
        }

        public async Task<ClubDto> CreateClubAsync(ClubDto clubDto)
        {
            if (clubDto == null) 
            { 
                 throw new ArgumentNullException(nameof(clubDto));
            } 
            var clubEntity = _mapper.Map<Club>(clubDto);
            await _clubRepository.AddAsync(clubEntity);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<ClubDto>(clubEntity); 
        }

        public async Task<bool> UpdateClubAsync(int id, ClubDto clubDto)
        {
             if (clubDto == null)
            { 
                 throw new ArgumentNullException(nameof(clubDto));
            } 
             if (id != clubDto.ClubId)
            {
                 throw new ArgumentException("Не збігаються ID клубу");
            }
            var existingClub = await _clubRepository.GetByIdAsync(id);
            if (existingClub == null)
            {
                return false;
            }
            _mapper.Map(clubDto, existingClub);
            _clubRepository.Update(existingClub);
            try
            {
                await _unitOfWork.SaveAsync();
                return true; 
            }
            catch(Exception)
            {
                return false; 
            }
        }

        public async Task<bool> DeleteClubAsync(int id)
        {
            var clubToDelete = await _clubRepository.GetByIdAsync(id);
            if (clubToDelete == null)
            {
                 return false; 
            }
            _clubRepository.Delete(clubToDelete); 
            try
            {
                await _unitOfWork.SaveAsync();
                return true; 
            }
            catch(Exception)
            {
                 return false; 
            }
        }
    }
}