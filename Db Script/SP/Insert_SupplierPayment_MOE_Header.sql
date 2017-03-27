USE [SHPP_TST]
GO
/****** Object:  StoredProcedure [dbo].[Update_SupplierPaymentReq_Header]    Script Date: 2017-03-27 10:08:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  procedure [dbo].[Insert_SupplierPayment_MOE_Header]
	@PaymentDate as datetime,
	@Year int,
	@Month  nvarchar(15),
	@BankCode as nvarchar(20),
	@BankName as nvarchar(50),
	@ChequeNumber as nvarchar(50) ,
	@ChequeDate as datetime,
	@Amount as decimal(18,2),
	@Status  nvarchar(15),
	@CreateUser  nvarchar(50),
	@CreateDate datetime = null,
	@AprovedUser nvarchar(50) = null,
	@ApprovedDate datetime =null,
	@New_PaymentReqNo int OUTPUT
AS
BEGIN

	Declare @CurrentStatus  nvarchar(10)

		INSERT INTO SupplierPayment_MOE_Header (PaymentDate ,Year,Month,BankCode,BankName,ChequeNumber,ChequeDate,Amount ,Status,CreateUser,CreateDate,ApprovedUser,ApprovedDate)
		VALUES (@PaymentDate, @Year,@Month ,@BankCode,@BankName,@ChequeNumber,@ChequeDate,@Amount ,@Status,@CreateUser, @CreateDate,@AprovedUser,@ApprovedDate)
		
		SET @New_PaymentReqNo =  SCOPE_IDENTITY()		

	    SELECT @New_PaymentReqNo = @New_PaymentReqNo
				
END


 --@return_value = EXEC Update_SupplierPaymentReq_Header 3, '2016/1/1', 1, '0101', 2016, 'December', 'Admin', 25000.00, 'NEW', 'Update', @New_PaymentReqNo