USE [SHPP_TST]
GO
/****** Object:  StoredProcedure [dbo].[Update_SupplierPaymentReq_Header]    Script Date: 2017-03-27 10:08:14 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  procedure [dbo].[Insert_SupplierPayment_MOE_Details]
	@PaymentID as decimal(18,2),
	@CensusId as nvarchar(50) ,
	@SupplierID as int,
	@BankCode  nvarchar(20),
	@BranchCode  nvarchar(20),
	@AccountNo as nvarchar(30),
	@Amount as decimal(18,2)
	
	AS
BEGIN

		INSERT INTO SupplierPayment_MOE_Details (PaymentID ,CensusID,SupplierID,BankCode,AccountNo,Amount,BranchCode)
		VALUES (@PaymentID, @CensusID,@SupplierID ,@BankCode,@AccountNo,@Amount,@BranchCode)							
END


 --@return_value = EXEC Update_SupplierPaymentReq_Header 3, '2016/1/1', 1, '0101', 2016, 'December', 'Admin', 25000.00, 'NEW', 'Update', @New_PaymentReqNo