USE [SHPP_TST]
GO
/****** Object:  StoredProcedure [dbo].[Update_SupplierPaymentReq_Details]    Script Date: 2017-03-30 11:59:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER procedure [dbo].[Update_SupplierPaymentReq_Details]
	@PaymentReqNo as decimal(8,0),
	@SupplyerID as int,
	@Amount as decimal(18,2),
	@Year int,
	@Month  nvarchar(15),
	@Paid AS BIT 
AS
BEGIN
	
	--Declare @TranType as nvarchar(10)

	Declare @BankCode as nvarchar(10)
	Declare @BranchCode as nvarchar(10)

	SELECT @BankCode = m_Banks.BankCode FROM m_Banks WHERE m_Banks.ID = 1
	SELECT @BranchCode = m_BanksBranch.BranchCode FROM m_BanksBranch WHERE m_BanksBranch.BankID = 1 AND m_BanksBranch.ID = 1

	IF @Amount > 0
	Begin
			
		INSERT INTO SupplierPaymentReq_Details(PaymentReqNo,SupplierID, CensusID, BankCode, BranchCode, AccountNo, Amount, Year, Month,Paid)
		VALUES        (@PaymentReqNo,@SupplyerID, '11111',@BankCode, @BranchCode, '111111', @Amount, @Year, @Month ,@Paid)

	End



END