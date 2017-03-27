USE [SHPP_TST]
GO
/****** Object:  StoredProcedure [dbo].[Update_SupplierPaymentReq_Header]    Script Date: 2017-03-26 10:19:09 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   procedure [dbo].[Update_SupplierPaymentReq_Header]
	@PaymentReqNo as decimal(8,0),
	@ReqDate as datetime,
	@ProvinceID as int,
	@ZoneID  nvarchar(10),
	@Year int,
	@Month  nvarchar(15),
	@CreateUser  nvarchar(50),
	@CreateDate datetime,
	@AprovedUser nvarchar(50),
	@ApprovedDate datetime,
	@ProvincialApp_User nvarchar(50),
	@ProvincialApp_Date datetime,
	@TotalAmount as decimal(18,2),
	@Status  nvarchar(15),
	@TranType as nvarchar(10),
	@New_PaymentReqNo int OUTPUT
AS
BEGIN

	Declare @CurrentStatus  nvarchar(10)

	If UPPER(@TranType) = 'NEW'
	Begin
		INSERT INTO SupplierPaymentReq_Header(ReqDate, ProvinceID, ZoneID, Year, Month, CreateUser, CreateDate,AprovedUser,ApprovedDate,ProvincialApp_User ,ProvincialApp_Date, TotalAmount,  Status)
		VALUES (CONVERT(DATETIME, @ReqDate, 102), @ProvinceID, @ZoneID, @Year,@Month , @CreateUser, @CreateDate,@AprovedUser,@ApprovedDate,@ProvincialApp_User ,@ProvincialApp_Date
		 ,@TotalAmount, @Status)
		
		SET @New_PaymentReqNo=  SCOPE_IDENTITY()
	
	End
	Else If UPPER(@TranType) = 'Update'
	Begin		
			UPDATE SupplierPaymentReq_Header SET 
				TotalAmount = @TotalAmount,
				Status = @Status,
				UpdateUser = @CreateUser,
				AprovedUser =@AprovedUser,
				ApprovedDate = @ApprovedDate,
				ProvincialApp_Date = @ProvincialApp_Date,
				ProvincialApp_User = @ProvincialApp_User,
				UpdateDate = GETDATE()
			WHERE  (PaymentReqNo = @PaymentReqNo)
		
		SET @New_PaymentReqNo =  @PaymentReqNo

		End

	 SELECT @New_PaymentReqNo = @New_PaymentReqNo
				
END


 --@return_value = EXEC Update_SupplierPaymentReq_Header 3, '2016/1/1', 1, '0101', 2016, 'December', 'Admin', 25000.00, 'NEW', 'Update', @New_PaymentReqNo
