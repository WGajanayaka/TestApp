USE [SHPP_DEV]
GO
/****** Object:  StoredProcedure [dbo].[Update_SupplierPaymentReq_Details]    Script Date: 8/15/2017 1:28:58 PM ******/
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
	Declare @AccountNumber as nvarchar(20)
	Declare @CensorsID as nvarchar(10)

	
	-- SELECT @BranchCode = m_BanksBranch.BranchCode FROM m_BanksBranch WHERE m_BanksBranch.BankID = 1 AND m_BanksBranch.ID = 1

	SELECT   @AccountNumber = m_SupplierInformation.BankAccountNo,    @BankCode = m_BanksBranch.BankCode, 
			@BranchCode = m_BanksBranch.BranchCode, @CensorsID = m_Schools.CensorsID
		FROM  m_SupplierInformation LEFT OUTER JOIN
              m_BanksBranch ON m_SupplierInformation.BankID = m_BanksBranch.BankID AND m_SupplierInformation.BankBranchID = m_BanksBranch.ID  INNER JOIN
                         dbo.m_Schools ON dbo.m_SupplierInformation.SchoolID = dbo.m_Schools.SchoolID
		WHERE (m_SupplierInformation.ID = @SupplyerID)

		
	IF @Amount > 0
	Begin
		
		
		INSERT INTO SupplierPaymentReq_Details(PaymentReqNo,SupplierID, CensusID, BankCode, BranchCode, AccountNo, Amount, Year, Month,Paid)
		VALUES        (@PaymentReqNo,@SupplyerID, @CensorsID, @BankCode, @BranchCode, @AccountNumber, @Amount, @Year, @Month ,@Paid)


	End



END

--  exec Update_SupplierPaymentReq_Details 5, 1554, 5000.00, 2017, 'June', 0
