﻿using Business.Abstract;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Utilities.BusinessRules;
using System.IO;
using System.Linq;

namespace Business.Concrete
{
    public class CarImageManager : ICarImageService
    {
        ICarImageDal _carImageDal;

        public CarImageManager(ICarImageDal carimageDal)
        {
            _carImageDal = carimageDal;
        }

        [ValidationAspect(typeof(CarImageValidator))]
        public IResult Add(CarImage carImage)
        {
            var result = BusinessRules.Run(CheckCarImageCount(carImage.CarId));
            
            if (result != null)
            {
                return result;
            }

            carImage.ImagePath = CreateImagePathAndSaveImage(carImage.ImagePath);
            carImage.ImageDate = DateTime.Now;

            _carImageDal.Add(carImage);

            return new SuccessResult(Messages.CarImageAdded);
        }

        public IResult Delete(CarImage carImage)
        {
            var image = _carImageDal.Get(c => c.CarImageId == carImage.CarImageId);
            
            if (image == null)
            {
                return new ErrorResult(Messages.CarImageNotFound);
            }

            File.Delete(image.ImagePath);

            _carImageDal.Delete(carImage);

            return new SuccessResult(Messages.CarImageDeleted);
        }

        public IResult Update(CarImage carImage)
        {
            var image = _carImageDal.Get(c => c.CarImageId == carImage.CarImageId);

            if (image == null)
            {
                return new ErrorResult(Messages.CarImageNotFound);
            }

            File.Delete(image.ImagePath);

            carImage.ImagePath = CreateImagePathAndSaveImage(carImage.ImagePath);
            carImage.ImageDate = DateTime.Now;

            _carImageDal.Update(carImage);

            return new SuccessResult(Messages.CarImageUpdated);
        }

        public IDataResult<List<CarImage>> GetAll()
        {
            return new SuccessDataResult<List<CarImage>>(_carImageDal.GetAll(), Messages.MessageListed);
        }

        public IDataResult<CarImage> GetById(int carImageId)
        {
            return new SuccessDataResult<CarImage>(_carImageDal.Get(ci => ci.CarImageId == carImageId));
        }


        // Create Image Path and Save Image Method
        private string CreateImagePathAndSaveImage(string imagePath)
        {
            string newImagePath;
            
            if (imagePath == null)
            {
                newImagePath = ImagePath.DefaultImagePath;
            }
            else
            {
                string guidKey = Guid.NewGuid().ToString();
                newImagePath = ImagePath.UploadImagePath + guidKey + ".jpg";
                File.Copy(imagePath, newImagePath);
            }

            return newImagePath;
        }

        // Business Rules Methods
        private IResult CheckCarImageCount(int carId)
        {
            if (_carImageDal.GetAll(ci => ci.CarId == carId).Count >= 5)
            {
                return new ErrorResult(Messages.CarImageNumberError);
            }
            return new SuccessResult();
        }


    }
}