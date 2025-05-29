//-----------------------------------------------------------------------
// <copyright file="ContractController.cs" company="LeaseAccelerator">
//     Copyright (c) Guardian Global Systems. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Guardian.PortfolioManager.Web.Controllers
{
    using CommonServiceLocator;
    using DocumentFormat.OpenXml.Packaging;
    using Guardian.Common.Web;
    using Guardian.Domain.Core;
    using Guardian.Domain.Core.Claims.AssetManagement.ContractsPermissions;
    using Guardian.Domain.Core.Claims.LeaseAccounting;
    using Guardian.Domain.Core.ConfigurationKeys;
    using Guardian.Domain.Core.Exceptions;
    using Guardian.Domain.Core.Extensions;
    using Guardian.Domain.Core.Resources.Labels;
    using Guardian.Domain.DTO;
    using Guardian.Domain.DTO.PortfolioManager;
    using Guardian.Domain.DTO.PortfolioManager.Assets.Edit;
    using Guardian.Domain.DTO.PortfolioManager.Assets.List;
    using Guardian.Domain.DTO.PortfolioManager.Assets.View;
    using Guardian.Domain.DTO.PortfolioManager.Contracts;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.AgreedValue.Edit;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.AgreedValue.List;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.AgreedValue.View;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.Base.Edit;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.Base.View;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.ContractType.Edit;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.Edit;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.LeaseAccounting;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.LeaseAccounting.Edit;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.LeaseAccounting.List;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.LeaseAccountingReview.Extensions;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.LeaseAccountingReview.List.Rules;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.Rate.Edit;
    using Guardian.Domain.DTO.PortfolioManager.Contracts.View;
    using Guardian.Domain.DTO.PortfolioManager.Extendable.Edit;
    using Guardian.Domain.DTO.PortfolioManager.Files.Edit;
    using Guardian.Domain.DTO.PortfolioManager.Helpers;
    using Guardian.Domain.DTO.PortfolioManager.Invoices.Edit;
    using Guardian.Domain.Entities.Modules.Auditing;
    using Guardian.Domain.Entities.Modules.Contracts;
    using Guardian.Domain.Entities.Modules.Extensibility;
    using Guardian.Domain.Interfaces.Constants;
    using Guardian.Domain.Interfaces.Services;
    using Guardian.Domain.Services.Extensions;
    using Guardian.Domain.Services.LeaseAccounting;
    using Guardian.Domain.Services.LeaseAccounting.Providers;
    using Guardian.Infrastructure.Common.Security;
    using Guardian.Infrastructure.Common.Util;
    using Guardian.Infrastructure.Data;
    using Guardian.Infrastructure.Data.Repository;
    using Guardian.LeaseAccelerator;
    using Guardian.PortfolioManager.Web.Models;
    using Guardian.PortfolioManager.Web.Models.Administration;
    using Guardian.PortfolioManager.Web.Models.Contracts;
    using Guardian.PortfolioManager.Web.Models.Contracts.AgreedValue;
    using Guardian.PortfolioManager.Web.Models.Contracts.AgreedValue.Edit;
    using Guardian.PortfolioManager.Web.Models.Contracts.Base;
    using Guardian.PortfolioManager.Web.Models.Contracts.Base.Edit;
    using Guardian.PortfolioManager.Web.Models.Contracts.Rate.Edit;
    using Guardian.PortfolioManager.Web.Models.Contracts.SubContracts;
    using Guardian.PortfolioManager.Web.Models.Invoices;
    using Guardian.PortfolioManager.Web.Models.LeaseAccounting;
    using Guardian.PortfolioManager.Web.Utility;
    using LA.Infrastructure.Claims.Helpers;
    using LeaseAcceleratorAPI.Constants;
    using LeaseAcceleratorAPI.Service;
    using Newtonsoft.Json;
    using OfficeOpenXml;
    using OpenXmlPowerTools;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Mime;
    using System.Web;
    using System.Web.Mvc;
    using System.Xml.Linq;
    using WebSupergoo.ABCpdf10;
    using static Guardian.Domain.DTO.PortfolioManager.Contracts.Base.View.PredefinedClauseViewModel;
    using static Guardian.Domain.Entities.Modules.Auditing.SystemAuditLogEntry;
    using static Guardian.PortfolioManager.Web.Models.Contracts.AgreedValue.Edit.VMActionAVReviewModel;
    using ExitCostEditModel = Guardian.Domain.DTO.PortfolioManager.Contracts.Edit.ExitCostEditModel;

    /// <summary>
    /// Defines the <see cref="ContractController" />.
    /// </summary>
    public class ContractController : AuthenticatedController
    {
        /// <summary>
        /// The asset service (readonly)..
        /// </summary>
        private readonly IAssetService assetService;

        /// <summary>
        /// The audit service (readonly)..
        /// </summary>
        private readonly IAuditService auditService;

        /// <summary>
        /// The contact service (readonly)..
        /// </summary>
        private readonly IContactService contactService;

        /// <summary>
        /// The contract service (readonly)..
        /// </summary>
        private readonly IContractService contractService;

        /// <summary>
        /// The AssetType service (readonly)
        /// </summary>
        private readonly IAssetTypesService assetTypesService;

        /// <summary>
        /// The contract type service (readonly)..
        /// </summary>
        private readonly IContractTypeService contractTypeService;

        /// <summary>
        /// The cost category service (readonly)..
        /// </summary>
        private readonly ICostCategoryService costCategoryService;

        /// <summary>
        /// The document service (readonly)..
        /// </summary>
        private readonly IDocumentService documentService;

        /// <summary>
        /// The extendable service (readonly)..
        /// </summary>
        private readonly IExtendableService extendableService;

        /// <summary>
        /// The file service (readonly)..
        /// </summary>
        private readonly IFileService fileService;

        /// <summary>
        /// The invoice service (readonly)..
        /// </summary>
        private readonly IInvoiceService invoiceService;

        /// <summary>
        /// The invoice type service (readonly)..
        /// </summary>
        private readonly IInvoiceTypeService invoiceTypeService;

        /// <summary>
        /// The lease accounting service (readonly)..
        /// </summary>
        private readonly ILeaseAccountingService leaseAccountingService;

        /// <summary>
        /// The locale service (readonly)..
        /// </summary>
        private readonly ILocaleService localeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractController"/> class.
        /// </summary>
        /// <param name="auditService">The auditService<see cref="IAuditService"/>.</param>
        /// <param name="contractService">The contractService<see cref="IContractService"/>.</param>
        /// <param name="assetService">The assetService<see cref="IAssetService"/>.</param>
        /// <param name="contacts">The contacts<see cref="IContactService"/>.</param>
        /// <param name="costs">The costs<see cref="ICostCategoryService"/>.</param>
        /// <param name="invoice">The invoice<see cref="IInvoiceService"/>.</param>
        /// <param name="locale">The locale<see cref="ILocaleService"/>.</param>
        /// <param name="contractType">The contractType<see cref="IContractTypeService"/>.</param>
        /// <param name="invoiceTypes">The invoiceTypes<see cref="IInvoiceTypeService"/>.</param>
        /// <param name="files">The files<see cref="IFileService"/>.</param>
        /// <param name="docs">The docs<see cref="IDocumentService"/>.</param>
        /// <param name="LeaseAccounting">The LeaseAccounting<see cref="ILeaseAccountingService"/>.</param>
        /// <param name="extendableService">The extendableService<see cref="IExtendableService"/>.</param>
        /// <param name="assetTypesService">The assetTypesService<see cref="IAssetTypesService"/>.</param>
        public ContractController(IAuditService auditService, IContractService contractService, IAssetService assetService, IContactService contacts, ICostCategoryService costs, IInvoiceService invoice, ILocaleService locale, IContractTypeService contractType, IInvoiceTypeService invoiceTypes, IFileService files, IDocumentService docs, ILeaseAccountingService LeaseAccounting, IExtendableService extendableService, IAssetTypesService assetTypesService)
        {
            this.contractService = contractService;
            this.assetService = assetService;
            contactService = contacts;
            costCategoryService = costs;
            invoiceService = invoice;
            localeService = locale;
            contractTypeService = contractType;
            invoiceTypeService = invoiceTypes;
            fileService = files;
            documentService = docs;
            leaseAccountingService = LeaseAccounting;
            this.auditService = auditService;
            this.extendableService = extendableService;
            this.assetTypesService = assetTypesService;
        }

        /// <summary>
        /// Gets a value indicating whether ContextAssetHasValue
        /// See if ContextAssetID has been set. if we try to get it and it doesn't exist it'll throw the ArgumentNull exception.
        /// </summary>
        public bool ContextAssetHasValue => !string.IsNullOrEmpty(Request["ContextID"]);

        /// <summary>
        /// Gets the TestAssetIsAccessible.
        /// </summary>
        public AccessCheck TestAssetIsAccessible => () => assetService.AssetIsAccessible(ContextAssetID);

        internal string FakeContextId = null;
        /// <summary>
        /// Gets the ContextAssetID.
        /// </summary>
        private int ContextAssetID
        {
            get
            {
                if (int.TryParse((FakeContextId ?? Request["ContextID"] ?? "").Split(',')[0], out int contextID) && contextID > 0)
                {
                    return contextID;
                }

                throw new ArgumentNullException(
                    @"Context AssetID must be specified and greater than 0, this must be tracked!",
                    new Exception(Request.Url == null ? "" : Request.Url.AbsoluteUri));
            }
        }

        private bool IsVaryLease
        {
            get
            {
                if (bool.TryParse(Request["IsVaryLease"], out bool IsVaryLease))
                {
                    return IsVaryLease;
                }
                return false;
            }
        }

        private string ReviewStartDateErrorMessage = "Date error. The Review start date cannot be the same as the Contract start date. Please adjust the dates and resubmit.";


        /// <summary>
        /// Used for ActionAVReview. Handles the case where we are actioning a new review with no underlying market, fixed, fixed% or CPI review. i.e. Adjustment and Commencing 
        /// </summary>
        /// <param name="reviewDate">Review Date of the review in question</param>
        /// <param name="terms">List of terms on the contract to get the start and end date</param>
        /// <param name="ParentContracts">List of Parent Contracts, if applicable, used for commencing review</param>
        /// <param name="type">String: should be Commencing or Adjustment, throws error otherwise</param>
        /// <param name="reviews">List of all reviews on contract</param>
        /// <param name="AssetSchedule">List of Asset Schedules on contract</param>
        /// <returns></returns>
        /// <exception cref="DomainValidationException"></exception>
        private VMActionAVReviewModel CreateAVReviewWithNoBaseReview(DateTime? reviewDate, List<TermEditModel> terms, List<VMParentContractsModel> ParentContracts, string type, List<VMAgreedValueReviewEditModel> reviews, List<ContractAssetScheduleItemEditModel> AssetSchedule)
        {

            DateTime start = terms[0].TermStart;
            DateTime? end = terms.Last().TermEnd;
            switch (type)
            {
                case "Commencing":
                    {
                        AssetListModel asset = assetService.GetAssetList(ContextAssetID);
                        var taxRateID = localeService.GetTaxRatesByJurisdiction(asset.DefaultJurisdictionCode).First().TaxRateID;
                        List<VMAgreedValueContractCostEditModel> defaultCosts = new List<VMAgreedValueContractCostEditModel>
                            {
                                new VMAgreedValueContractCostEditModel
                                {
                                    AssetID = ContextAssetID,
                                    FirstPaymentDate = start,
                                    PaymentFrequency = 1,
                                    PaymentPattern = "Months",
                                    JurisdictionCode = asset.DefaultJurisdictionCode,
                                    TaxRateID =taxRateID
                                }
                            };
                        if (ParentContracts?.Count > 0)
                        {
                            defaultCosts = ParentContracts
                                .SelectMany(pc => pc.SubContractMappings
                                    .Where(sc => sc.SubcontractedEnabled)
                                    .Select(map => new VMAgreedValueContractCostEditModel
                                    {
                                        AssetID = map.AssetID,
                                        FirstPaymentDate = terms[0].TermStart,
                                        PaymentFrequency = 1,
                                        PaymentPattern = "Months",
                                        //assume child assets have the same juridsdiction as the parent asset
                                        JurisdictionCode = asset.DefaultJurisdictionCode,
                                        TaxRateID = taxRateID
                                    }))
                        .ToList();
                        }
                        else if (AssetSchedule != null && AssetSchedule.Count > 0)
                        {
                            defaultCosts = AssetSchedule
                                   .Select(map => new VMAgreedValueContractCostEditModel
                                   {
                                       AssetID = map.AssetID,
                                       FirstPaymentDate = terms.First().TermStart,
                                       PaymentFrequency = 1,
                                       PaymentPattern = "Months",
                                       //assume child assets have the same juridsdiction as the parent asset
                                       JurisdictionCode = asset.DefaultJurisdictionCode,
                                       TaxRateID = taxRateID
                                   })
                           .ToList();
                        }
                        return new VMActionAVReviewModel
                        {
                            Guid = "costs",
                            ActionedDate = start,
                            ContractStart = start,
                            ContractEnd = end,
                            EffectiveDate = start,
                            Priority = 0,
                            ReviewID = -1,
                            ReviewType = type,
                            ReviewDate = start,
                            MinimumEffectiveDate = start,
                            Templates = new List<VMActionAVReviewModel.VMActionAVReviewTemplateModel>(),
                            ActionedCosts_NotInvoiced = defaultCosts
                        };
                    }
                case "Adjustment":
                    if (reviews.All(r => r.ReviewType != "Commencing"))
                    {
                        throw new DomainValidationException("Commencing costs must be configured before adding cost adjustments");
                    }

                    if (reviewDate < start)
                    {
                        throw new DomainValidationException("Please select a date on or after the contract commencement date (" + start.ToString(UserContext.Current.DateFormat) + ")");
                    }

                    if (reviewDate == null)
                    {
                        throw new DomainValidationException("Please select an effective date for the cost adjustment");
                    }

                    VMAgreedValueReviewEditModel lastActionedReview = reviews.Where(r => r.ActionedReview != null).OrderBy(r => r.ActionedReview.EffectiveDate).ThenBy(r => r.ActionedReview.Priority).Last();
                    int priority = lastActionedReview.ActionedReview.EffectiveDate == reviewDate ? lastActionedReview.ActionedReview.Priority + 1 : 0;


                    List<CostTemplateMapping> costs = lastActionedReview
                        .ActionedReview
                        .Templates
                        .SelectMany(t => t.ActionedCosts
                            .Union(t.UnchangedCosts)
                            .Select(c => new CostTemplateMapping { Cost = c, Template = t })
                        ).ToList();

                    costs.AddRange(lastActionedReview.ActionedReview.ActionedCosts_NotInvoiced.Select(c => new CostTemplateMapping { Cost = c, Template = null }));
                    costs.AddRange(lastActionedReview.ActionedReview.UnactionedCosts_NotInvoiced.Select(c => new CostTemplateMapping { Cost = c, Template = null }));
                    costs.AddRange(lastActionedReview.ActionedReview.UnchangedTemplates.SelectMany(t => t.UnchangedCosts.Select(c => new CostTemplateMapping { Cost = c, Template = t })));

                    VMActionAVReviewModel model = new VMActionAVReviewModel
                    {
                        ActionedDate = DateTime.Now,
                        ActionedCosts_NotInvoiced = new List<VMAgreedValueContractCostEditModel>(
                            lastActionedReview.ActionedReview.ActionedCosts_NotInvoiced.Union(lastActionedReview
                                .ActionedReview.UnchangedCosts)),
                        UnchangedTemplates =
                            new List<VMActionAVReviewModel.VMActionAVReviewTemplateModel>(lastActionedReview
                                .ActionedReview.Templates),
                        UnchangedCosts =
                            new List<VMAgreedValueContractCostEditModel>(lastActionedReview.ActionedReview
                                .UnchangedCosts),
                        ContractEnd = end,
                        ContractStart = start,
                        EffectiveDate = reviewDate.Value,
                        IsNew = true,
                        Priority = priority,
                        ReviewType = type,
                        ReviewID = -1,
                        ReviewDate = reviewDate.Value,
                        MinimumEffectiveDate = lastActionedReview.ActionedReview.EffectiveDate,
                        Guid = Guid.NewGuid().ToString()
                    };
                    SetReviewCostIDsToNegativeOne(model);
                    model.CloneAllCosts().ForEach(c => c.SetOld());
                    return model;

                default:
                    // we should never get here, the only two types of actioned av reviews
                    // created on the fly are commencing and cost adjustments
                    throw new DomainValidationException("A new actioned review of the specified type cannot be created. Please check the contract structure has been properly defined");
            }

        }

        /// <summary>
        /// Used for ActionAVReview. Takes a model and sets the IDs to -1 for CostID, template cost ID 
        /// </summary>
        /// <param name="model"></param>
        private void SetReviewCostIDsToNegativeOne(VMActionAVReviewModel model)
        {
            model.UnchangedTemplates.ForEach(t =>
            {
                if (t == null)
                {
                    return;
                }

                t.UnchangedCosts.AddRange(t.ActionedCosts);
                t.ActionedCosts = new List<VMAgreedValueContractCostEditModel>();
            });
            model.Templates.SelectMany(t => t.ActionedCosts).ToList().ForEach(c =>
            {
                c.CostID = -1;
                c.TemplateCostID = -1;
                c.Actioned = true;
            });
            model.ActionedCosts_NotInvoiced.ForEach(c =>
            {
                c.CostID = -1;
                c.TemplateCostID = -1;
                c.Actioned = true;
            });
            model.UnactionedCosts_NotInvoiced.ForEach(c =>
            {
                c.CostID = -1;
                c.TemplateCostID = -1;
                c.Actioned = false;
            });
        }

        /// <summary>
        /// Used for ActionAVReview. Takes the current VMAgreedValueContractCostEditModel and the previous one and copies over the values
        /// </summary>
        /// <param name="currentCost"></param>
        /// <param name="cpiregions"></param>
        /// <param name="review"></param>
        /// <param name="rc"></param>
        void UpdateNewActionedCost(VMAgreedValueContractCostEditModel currentCost, Dictionary<string, string> cpiregions, VMAgreedValueReviewEditModel review, VMActionAVReviewTemplateModel template = null)
        {
            //Market/CPI does not save the label so the estimate should apply to all costs matching AssetID and categoryID if it's a CPI Review
            AgreedValueReviewCostEditModel rc = review.Costs.FirstOrDefault(c2 => c2.AssetID == currentCost.AssetID && c2.CategoryID == currentCost.CategoryID && (review.ReviewType == "CPI" || review.ReviewType == "Market" || c2.Label == currentCost.Label));
            currentCost.OldYearlyAmount = currentCost.YearlyAmount;
            currentCost.OldPaymentAmount = currentCost.PaymentAmount;
            currentCost.OldPaymentPattern = currentCost.PaymentPattern;
            currentCost.OldPaymentFrequency = currentCost.PaymentFrequency;
            currentCost.OldTaxAmount = currentCost.TaxAmount;
            currentCost.OldJurisdictionCode = currentCost.JurisdictionCode;
            currentCost.OldTaxRateID = currentCost.TaxRateID;
            currentCost.OldPaidInArrears = currentCost.PaidInArrears;
            currentCost.Cap = rc?.Cap ?? 0;
            currentCost.Collar = rc?.Collar ?? 0;
            currentCost.Estimate = rc?.Estimate ?? 0;
            currentCost.Plus = rc?.Plus ?? 0;
            //This is ok to be 0 for fixed reviews
            currentCost.Actual = (review.ReviewType == "Fixed%") ? rc?.FixedPercent ?? 0 : (rc?.Estimate ?? 0) + (rc?.Plus ?? 0);
            if (rc != null && rc.CPIRegionID.HasValue && rc.CPIRegionID.Value > 0)
            {
                currentCost.CPIRegion = cpiregions[rc.CPIRegionID.Value.ToString()];
            }
            VMAgreedValueContractCostEditModel oldReviewCost = null;
            //Check if we had an old actioned review or not                                 
            if (review.OldActionedReview != null)
            {
                List<VMAgreedValueContractCostEditModel> AllOldReviewCosts = new List<VMAgreedValueContractCostEditModel>();
                AllOldReviewCosts.AddRange(review.OldActionedReview.Templates.SelectMany(t => t.ActionedCosts));
                AllOldReviewCosts.AddRange(review.OldActionedReview.ActionedCosts_NotInvoiced);

                oldReviewCost = AllOldReviewCosts.FirstOrDefault(oc => oc.AssetID == currentCost.AssetID && oc.CategoryID == currentCost.CategoryID && oc.Label == currentCost.Label);
                if (oldReviewCost != null)
                {
                    currentCost.PaymentAmount = oldReviewCost.PaymentAmount;
                    currentCost.TaxAmount = oldReviewCost.TaxAmount;
                    currentCost.YearlyAmount = oldReviewCost.YearlyAmount;
                    currentCost.PaymentPattern = oldReviewCost.PaymentPattern;
                    currentCost.PaymentFrequency = oldReviewCost.PaymentFrequency;
                    currentCost.JurisdictionCode = oldReviewCost.JurisdictionCode;
                    currentCost.TaxRateID = oldReviewCost.TaxRateID;
                    currentCost.PaidInArrears = oldReviewCost.PaidInArrears;
                    currentCost.Actual = oldReviewCost.Actual;
                    currentCost.FirstPaymentDate = oldReviewCost.FirstPaymentDate;

                    var oldTemplate = review.OldActionedReview.Templates.FirstOrDefault(t => t.Guid == oldReviewCost.TemplateGuid || t.InvoiceTemplateID == oldReviewCost.OriginalTemplateID);

                    if (template != null && oldTemplate != null)
                    {
                        template.Description = oldTemplate.Description;
                        template.FirstInvoiceDate = oldTemplate.FirstInvoiceDate;
                        template.Frequency = oldTemplate.Frequency;
                        template.InvoiceGroup = oldTemplate.InvoiceGroup;
                        template.Guid = oldTemplate.Guid;
                        template.InvoiceTemplateID = oldTemplate.InvoiceTemplateID;
                        template.InvoiceTypeID = oldTemplate.InvoiceTypeID;
                        template.Pattern = oldTemplate.Pattern;
                        //TODO: should these also be added as part of LA-42937 / LA-62293?
                        //template.TemplateVendorID = oldTemplate.TemplateVendorID;
                        //template.TemplateVendorName = oldTemplate.TemplateVendorName;

                    }
                }
            }
            else
            {
                switch (review.ReviewType)
                {
                    case "Fixed":
                        currentCost.PaymentAmount = rc?.PaymentAmount ?? currentCost.PaymentAmount;
                        break;
                    case "Fixed%":
                        currentCost.PaymentAmount = currentCost.PaymentAmount * (1 + ((rc?.FixedPercent ?? 0) / 100M));
                        break;
                    case "Market":
                    case "CPI":
                        currentCost.PaymentAmount = currentCost.PaymentAmount * (1 + (currentCost.Actual / 100));
                        break;
                }
                currentCost.TaxAmount = localeService.GetTaxRatesByJurisdiction(currentCost.JurisdictionCode).First(t => t.TaxRateID == currentCost.TaxRateID).History.First(h => (h.ValidFrom ?? DateTime.MinValue) <= review.ReviewDate && (h.ValidTo ?? DateTime.MaxValue) >= review.ReviewDate).Multiplier * currentCost.PaymentAmount;
                switch (currentCost.PaymentPattern)
                {
                    case "Weeks":
                        currentCost.YearlyAmount = currentCost.PaymentAmount * (52M / currentCost.PaymentFrequency);
                        break;

                    case "Months":
                        currentCost.YearlyAmount = currentCost.PaymentAmount * (12M / currentCost.PaymentFrequency);
                        break;

                    case "Quarters":
                        currentCost.YearlyAmount = currentCost.PaymentAmount * (4M / currentCost.PaymentFrequency);
                        break;

                    case "Years":
                        currentCost.YearlyAmount = currentCost.PaymentAmount;
                        break;
                }
            }

        }


        /// <summary>
        /// The ActionAVReview.
        /// </summary>
        /// <param name="reviews">The reviews<see cref="List{VMAgreedValueReviewEditModel}"/>.</param>
        /// <param name="type">The type<see cref="string"/>.</param>
        /// <param name="guid">The guid<see cref="string"/>.</param>
        /// <param name="terms">The terms<see cref="List{TermEditModel}"/>.</param>
        /// <param name="currencyID">The currencyID<see cref="int"/>.</param>
        /// <param name="templates">The templates<see cref="List{VMInvoiceTemplateEditModel}"/>.</param>
        /// <param name="reviewDate">The reviewDate<see cref="DateTime?"/>.</param>
        /// <param name="IsInHoldover">The IsInHoldover<see cref="bool"/>.</param>
        /// <param name="ParentContracts">The ParentContracts<see cref="List{VMParentContractsModel}"/>.</param>
        /// <param name="AssetSchedule">The AssetSchedule<see cref="List{ContractAssetScheduleItemEditModel}"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        [JsonNetHandler(typeof(VMTaxRateViewModel.VMTaxRateListJsonConverter))]
        public ExtendedJsonResult ActionAVReview(List<VMAgreedValueReviewEditModel> reviews, string type, string guid, List<TermEditModel> terms, int currencyID, List<VMInvoiceTemplateEditModel> templates, DateTime? reviewDate, bool IsInHoldover, List<VMParentContractsModel> ParentContracts, List<ContractAssetScheduleItemEditModel> AssetSchedule, int VendorId, bool? IsSubjectToLeaseAccounting = false, bool IsNewCost = true)
        {
            ModelState.Clear();
            //No matter what the result we need this to be returned
            ViewBag.ContextID = ContextAssetID;
            ViewBag.VaryLease = Request["VaryLease"] == "True";
            ViewBag.IsNewCost = IsNewCost;
            ViewBag.VendorID = VendorId;

            terms = (terms ?? new List<TermEditModel>()).OrderBy(t => t.TermStart).ToList();
            if (terms.Count < 1)
            {
                return ExtendedJson(new { success = false, message = "An initial term must be added to the contract before costs and reviews can be defined" });
            }
            if (type != "Commencing" && type != "Adjustment" && !reviews.Any(r => r.Guid == guid))
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new InvalidOperationException($"Attempted to action a {type} review using {guid} however no such review exists in the reviews collection ({reviews.Count} reviews submitted)"));
                return ExtendedJson(new
                {
                    success = false,
                    message = "Unable to action review - an unexpected error occurred. Please try again."
                });
            }

            if (IsSubjectToLeaseAccounting != null)
            {
                ViewBag.SubjectToLeaseAccounting = IsSubjectToLeaseAccounting;
            }

            reviews = (reviews ?? new List<VMAgreedValueReviewEditModel>()).OrderBy(r => r.ReviewDate).ToList();

            IEnumerable<CostCategoryListModel> categories = costCategoryService.GetAllCostCategories();
            IDictionary<string, TaxJurisdictionViewModel> jurisdictions = localeService.GetTaxJurisdictions();
            Dictionary<string, string> cpiregions = contractService.GetCPIRegionList().ToDictionary(r => r.ID.ToString(), r => r.Name);
            DateTime start = terms[0].TermStart;
            DateTime? end = terms.Last().TermEnd;
            VMActionAVReviewModel model;

            if (string.IsNullOrWhiteSpace(guid))
            {
                try
                {
                    model = CreateAVReviewWithNoBaseReview(reviewDate, terms, ParentContracts, type, reviews, AssetSchedule);
                    //remove the blank cost only add when needed via UI
                    if (model.ActionedCosts_NotInvoiced.Any())
                    {
                        if (model.ActionedCosts_NotInvoiced[0].Category == null &&
                            model.ActionedCosts_NotInvoiced[0].CategoryID == 0 &&
                            model.ActionedCosts_NotInvoiced[0].PaymentAmount == 0)
                        {
                            model.ActionedCosts_NotInvoiced.RemoveAt(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return ExtendedJson(new AjaxResponse { message = ex.Message, success = false });
                }
            }
            else
            {
                // we're actioning a review or editing an already actioned review
                VMAgreedValueReviewEditModel review = reviews.Single(r => r.Guid == guid);
                reviewDate = review.ReviewDate;
                VMAgreedValueReviewEditModel lastActionedReview = reviews.Where(r => r.ActionedReview != null).OrderBy(r => r.ActionedReview.EffectiveDate).ThenBy(r => r.ActionedReview.Priority).Last(r => r.ReviewDate <= reviewDate);
                if (review.ActionedReview != null)
                {
                    if (review.ReviewType == "Adjustment" && (review.ActionedReview.EffectiveDate < lastActionedReview.ActionedReview.EffectiveDate || (review.ActionedReview.EffectiveDate == lastActionedReview.ActionedReview.EffectiveDate && review.ActionedReview.Priority < lastActionedReview.ActionedReview.Priority)))
                    {
                        return ExtendedJson(new
                        {
                            success = false,
                            message = "Only cost adjustments after the last actioned review may be edited. To edit this cost adjustment, revert any successive actioned reviews until the last actioned review occurrs before this cost adjustment"
                        });
                    }

                    if (review.ReviewType != "Adjustment" && review.ReviewType != "Commencing" && review != lastActionedReview)
                    {
                        return ExtendedJson(new
                        {
                            success = false,
                            message = "Only the last actioned review may be edited. To edit this actioned review, revert any successive actioned reviews until this review is the last actioned review"
                        });
                    }

                    model = review.ActionedReview;
                    model.ContractStart = start;
                    model.ContractEnd = end;
                    model.Notes = review.Notes;
                    if (guid == "costs")
                    {
                        model.MinimumEffectiveDate = start;
                    }
                    else
                    {
                        VMAgreedValueReviewEditModel previousReview = reviews.Where(r => r.ActionedReview != null).OrderByDescending(r => r.ActionedReview.EffectiveDate).ThenByDescending(r => r.ActionedReview.Priority).Where(r => r.ReviewDate <= reviewDate).Skip(1).First();
                        model.MinimumEffectiveDate = previousReview.ActionedReview.EffectiveDate;
                    }
                }
                else
                {
                    // actioning a review
                    if (review.ReviewDate < lastActionedReview.ReviewDate)
                    {
                        return ExtendedJson(new
                        {
                            success = false,
                            message = "Only reviews after the last actioned review may be actioned. To action this review, revert any successive actioned reviews until there are no actioned reviews after this review"
                        });
                    }
                    DateTime? exercisedEnd = terms.Last(t => !t.IsOption || t.State == "Exercised").TermEnd;
                    if (exercisedEnd.HasValue && exercisedEnd.Value > DateTime.MinValue && review.ReviewDate > exercisedEnd.Value && !IsInHoldover)
                    {
                        return ExtendedJson(new
                        {
                            success = false,
                            message = "Only reviews within the contract exercised term range may be actioned. To action this review, first exercise options until this review falls within the contract exercised term range"
                        });
                    }
                    int priority = reviews.Where(r => r.ActionedReview != null).Any(r => r.ActionedReview.EffectiveDate.Date == reviewDate.Value.Date) ? reviews.Where(r => r.ActionedReview != null && r.ActionedReview.EffectiveDate.Date == reviewDate.Value.Date).Max(r => r.ActionedReview.Priority) + 1 : 0;
                    lastActionedReview.ActionedReview.Templates.ForEach(r =>
                    {
                        r.VendorID = r.TemplateVendorID;
                        r.VendorName = r.TemplateVendorName;
                    });
                    List<CostTemplateMapping> costs = lastActionedReview
                        .ActionedReview
                        .Templates
                        .SelectMany(t => t.ActionedCosts
                            .Union(t.UnchangedCosts)
                            .Select(c => new CostTemplateMapping { Cost = c, Template = t })
                        ).ToList();
                    costs.AddRange(lastActionedReview.ActionedReview.UnchangedCosts.Select(c => new CostTemplateMapping { Cost = c, Template = null }));
                    costs.AddRange(lastActionedReview.ActionedReview.ActionedCosts_NotInvoiced.Select(c => new CostTemplateMapping { Cost = c, Template = null }));
                    costs.AddRange(lastActionedReview.ActionedReview.UnactionedCosts_NotInvoiced.Select(c => new CostTemplateMapping { Cost = c, Template = null }));
                    costs.AddRange(lastActionedReview.ActionedReview.UnchangedTemplates.SelectMany(t => t.UnchangedCosts.Select(c => new CostTemplateMapping { Cost = c, Template = t })));

                    List<CostTemplateMapping> set1 = new List<CostTemplateMapping>();
                    List<CostTemplateMapping> set2 = new List<CostTemplateMapping>();
                    List<CostTemplateMapping> set3 = new List<CostTemplateMapping>();

                    switch (review.ReviewType)
                    {
                        case "Fixed":
                        case "Fixed%":
                            set1.AddRange(costs.Where(c => review.Costs.Any(c2 => c2.AssetID == c.Cost.AssetID && c2.CategoryID == c.Cost.CategoryID && c2.Label == c.Cost.Label)));
                            set2.AddRange(costs.Where(c => !set1.Contains(c) && set1.Any(c2 => c.Template != null && c2.Template == c.Template)));
                            set3.AddRange(costs.Where(c => !set1.Contains(c) && !set2.Contains(c)));
                            break;

                        case "Market":
                        case "CPI":
                            set1.AddRange(costs.Where(c => review.Costs.Any(c2 => c2.AssetID == c.Cost.AssetID && c2.CategoryID == c.Cost.CategoryID)));
                            set2.AddRange(costs.Where(c => !set1.Contains(c) && set1.Any(c2 => c.Template != null && c2.Template == c.Template)));
                            set3.AddRange(costs.Where(c => !set1.Contains(c) && !set2.Contains(c)));
                            break;
                    }
                    int actionedMin = reviews.Where(r => r.ActionedReview != null).Min(r => r.ActionedReview.Templates.DefaultIfEmpty(new VMActionAVReviewModel.VMActionAVReviewTemplateModel { InvoiceTemplateID = 0 }).Min(t => t.InvoiceTemplateID));
                    int unchangedMin = reviews.Where(r => r.ActionedReview != null).Min(r => r.ActionedReview.UnchangedTemplates.DefaultIfEmpty(new VMActionAVReviewModel.VMActionAVReviewTemplateModel { InvoiceTemplateID = 0 }).Min(t => t.InvoiceTemplateID));
                    int ntid = Math.Min(0, actionedMin < unchangedMin ? actionedMin : unchangedMin);

                    model = new VMActionAVReviewModel
                    {
                        ActionedDate = DateTime.Now,
                        ActionedCosts_NotInvoiced = set1.Where(c => c.Template == null).Select(c =>
                        {
                            c.Cost.FirstPaymentDate = FirstPaymentDateByPatternAndFrequency(start, c.Cost.FirstPaymentDate, reviewDate.Value, c.Cost.PaymentFrequency, c.Cost.PaymentPattern, true);
                            c.Cost.Actioned = true;
                            return c.Cost;
                        }).ToList(),
                        UnchangedTemplates = set3.Where(c => c.Template != null).Select(c => c.Template).Distinct().ToList(),
                        UnchangedCosts = set3.Where(c => c.Template == null).Select(c =>
                        {
                            c.Cost.FirstPaymentDate = FirstPaymentDateByPatternAndFrequency(start, c.Cost.FirstPaymentDate, reviewDate.Value, c.Cost.PaymentFrequency, c.Cost.PaymentPattern, true);
                            return c.Cost;
                        }).ToList(),
                        ContractEnd = end,
                        ContractStart = start,
                        EffectiveDate = reviewDate.Value,
                        IsNew = true,
                        Priority = priority,
                        ReviewType = type,
                        RemeasurementDate = review.RemeasurementDate,
                        ReviewID = -1,
                        ReviewDate = reviewDate.Value,
                        Notes = review.Notes,
                        MinimumEffectiveDate = lastActionedReview.ActionedReview.EffectiveDate,
                        Guid = guid,
                        Templates = set1.Where(c => c.Template != null).GroupBy(c => c.Template.InvoiceTemplateID).Select(t =>
                        {
                            VMActionAVReviewModel.VMActionAVReviewTemplateModel tem = new VMActionAVReviewModel.VMActionAVReviewTemplateModel
                            {
                                Description = t.First().Template.Description,
                                FirstInvoiceDate = FirstPaymentDateByPatternAndFrequency(start, t.First().Template.FirstInvoiceDate, reviewDate.Value, t.First().Template.Frequency, t.First().Template.Pattern, true),
                                Frequency = t.First().Template.Frequency,
                                InvoiceGroup = t.First().Template.InvoiceGroup,
                                Guid = Guid.NewGuid().ToString(),
                                InvoiceTemplateID = --ntid,
                                InvoiceTypeID = t.First().Template.InvoiceTypeID,
                                Pattern = t.First().Template.Pattern,
                                VendorID = t.First().Template.VendorID,
                                VendorName = t.First().Template.VendorName,
                                TemplateVendorID = t.First().Template.VendorID,
                                TemplateVendorName = t.First().Template.VendorName,
                            };
                            tem.ActionedCosts = t.Select(c =>
                            {
                                c.Cost.FirstPaymentDate = FirstPaymentDateByPatternAndFrequency(start, c.Cost.FirstPaymentDate, reviewDate.Value, c.Cost.PaymentFrequency, c.Cost.PaymentPattern, true);
                                c.Cost.Actioned = true;
                                c.Cost.TemplateGuid = tem.Guid;
                                return c.Cost;
                            }).ToList();
                            tem.UnchangedCosts = set2.Where(c => c.Template.InvoiceTemplateID == t.Key).Select(c =>
                            {
                                c.Cost.FirstPaymentDate = FirstPaymentDateByPatternAndFrequency(start, c.Cost.FirstPaymentDate, reviewDate.Value, c.Cost.PaymentFrequency, c.Cost.PaymentPattern, true);
                                c.Cost.Actioned = false;
                                c.Cost.TemplateGuid = tem.Guid;
                                return c.Cost;
                            }).ToList();
                            return tem;
                        }).ToList()
                    };
                    model.UnchangedTemplates.ForEach(t =>
                    {
                        if (t == null)
                        {
                            return;
                        }

                        t.UnchangedCosts.AddRange(t.ActionedCosts);
                        t.ActionedCosts = new List<VMAgreedValueContractCostEditModel>();
                    });
                    SetReviewCostIDsToNegativeOne(model);
                    model.Templates.ForEach(tem => tem.ActionedCosts.ForEach(c =>
                    {
                        UpdateNewActionedCost(c, cpiregions, review, tem);
                    }));
                    model.ActionedCosts_NotInvoiced.ForEach(c =>
                    {
                        UpdateNewActionedCost(c, cpiregions, review, null);
                    });

                }
            }
            List<string> groups = invoiceService.GetAllInvoiceGroups().Where(g => !string.IsNullOrWhiteSpace(g)).ToList();
            groups.Add(ClientContext.Current.GetConfigurationSetting("Invoices.DefaultGroup", "Basic Invoice"));
            groups.AddRange(model.Templates.Select(t => t.InvoiceGroup).Where(g => !string.IsNullOrWhiteSpace(g)));
            groups = groups.Distinct().OrderBy(g => g, StringComparer.OrdinalIgnoreCase).ToList();
            ViewBag.InvoiceGroups = groups.Select(g => new SelectListItem
            {
                Text = g,
                Value = g
            }).ToList();
            ViewBag.InvoiceTypes = invoiceTypeService.GetInvoiceTypes().Select(t => new SelectListItem
            {
                Text = t.Name,
                Value = t.InvoiceTypeID.ToString()
            }).ToList();

            List<SelectItem> assetlist = assetService.GetAssetSelectList(null, false);
            if (ParentContracts?.Count > 0)
            {
                assetlist = assetlist.Where(a => ParentContracts.Any(sc => sc.SubContractMappings
                    //for a subcontract we want either the asset,parent or context asset so that things don't break
                    .Any(sm => sm.AssetID.ToString() == a.Key || sm.ParentAssetID.ToString() == a.Key
                    || ContextAssetID.ToString() == a.Key))).ToList();
                int tempid = -1;
                assetlist.AddRange(ParentContracts.SelectMany(pc => pc.SubContractMappings)
                    .Where(sm => sm.SubContractOptions == VMSubContractMappingModel.SubContractAssetOptions.CreateNewAsset)
                    .Select(sm => new SelectItem { Key = tempid--.ToString(), Name = sm.ChildAssetDetails.Name, Visible = true }));
            }
            if (AssetSchedule != null && AssetSchedule.Count > 0)
            {
                if (reviewDate == null)
                    reviewDate = DateTime.MinValue;

                assetlist = assetlist.Where(a => ContextAssetID.ToString() == a.Key || AssetSchedule.Any(sc => sc
                    //for a subcontract we want either the asset,parent or context asset so that things don't break
                    .AssetID.ToString() == a.Key && (sc.ValidFrom ?? DateTime.MinValue) <= reviewDate && (sc.ValidTo ?? DateTime.MaxValue) >= reviewDate)).ToList();
            }

            return ExtendedJson(new
            {
                success = true,
                type,
                UsePAIncTax = ContractOptions.Get<bool>(ContractOptions.ShowPAIncTax),
                html = RenderVariantPartialViewToString("Partial/ActionAVReview", model),
                cpiregions,
                categories = categories.ToDictionary(c => c.CostCategoryID.ToString(), c => c.DisplayName()),
                assets = assetlist,
                jurisdictions = localeService.GetTaxJurisdictions().Values.ToDictionary(j => j.Code, j => new
                {
                    code = j.Code,
                    name = j.Name,
                    taxrates = (IList<VMTaxRateViewModel>)null
                }),
                groups = invoiceService.GetAllInvoiceGroups().Union(model.Templates.Select(t => t.InvoiceGroup)).Where(g => !string.IsNullOrEmpty(g)).OrderBy(g => g).ToList(),
                invoicetypes = invoiceTypeService.GetInvoiceTypes().ToDictionary(t => t.InvoiceTypeID.ToString(), t => t.Name)
            });


        }

        /// <summary>
        /// The ContractFilesStructure
        /// </summary>
        /// <param name="id">The ID<see cref="int"/></param>
        /// <returns>The <see cref="ActionResult"/></returns>
        public ActionResult ContractFilesStructure(int typeId, string subType, Guid EntityID)
        {
            ContractTypeEditModel contracttype = contractTypeService.GetContractType(typeId);
            FolderStructure[] defaultfolders = contracttype.DefaultFolders.ContainsKey(subType) ? contracttype.DefaultFolders[subType] : Array.Empty<FolderStructure>();

            List<FileEditModel> contractFiles = fileService.GetEntityFiles(EntityID);

            ViewBag.AllowUpload = true;
            ViewBag.ShowMoveFolderToAssetOption = false;
            ViewBag.ShowMoveFileToAssetOption = false;
            ViewBag.AllowRemove = true;
            ViewBag.SaveImmediately = true;

            FilesController.FileFolderStructure structure = new FilesController.FileFolderStructure
            {
                EntityID = EntityID,
                Path = new List<string>(),
                Files = new List<FileEditModel>(),
                Children = defaultfolders.Select(c => ContractTypesController.ContractTypeFolderStructure(new List<string>(), c, EntityID)).ToList()
            };

            FilesController.FileFolderStructure fileStructure = FilesController.FolderStructure(EntityID, new List<string>(), "", contractFiles);
            FilesController.MergeFileStructures(structure, fileStructure);

            return View("FileHeirarchy", structure);
        }

        /// <summary>
        /// The ActionNextAVReview.
        /// </summary>
        /// <param name="contractID">The contractID<see cref="int"/>.</param>
        /// <param name="reviewid">The reviewid<see cref="int"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        [JsonNetHandler(typeof(VMTaxRateViewModel.VMTaxRateListJsonConverter))]
        public ExtendedJsonResult ActionNextAVReview(int contractID, bool? IsSubjectToLeaseAccounting = false)
        {
            AgreedValueContractEditModel contract = contractService.GetContractEdit(contractID) as AgreedValueContractEditModel;
            if (contract == null)
            {
                return ExtendedJson(new { success = false, message = "The contract you are trying to edit may have been deleted." }, JsonRequestBehavior.AllowGet);
            }

            var review = contract.NextReview();
            VMAgreedValueContractEditModel vm = MapAgreedValueContractToVM(contract);
            VMAgreedValueReviewEditModel nextreview = vm.Reviews.SingleOrDefault(r => r.ReviewID == review.ReviewID);
            //TODO: this would be more efficient to incorporate with the other stuff thats already loaded for the page; but currently just calling this inline if it might need the remeasurement date field
            ViewBag.IsLASigSyncdWithLAReviews = new Lazy<bool>(() =>
            {
                return contract.SubjectToLeaseAccounting && LeaseAccountingProviderFactory.Current.LeaseAccountingReviewForContractEverDone(contractID);
            });

            if (nextreview == null)
            {
                return ExtendedJson(new { success = false, message = "Couldn't find the next review to action" }, JsonRequestBehavior.AllowGet);
            }

            TermEditModel nextOption = contract.NextOption();
            if (nextOption != null && nextOption.TermStart == nextreview.ReviewDate)
            {
                nextOption.State = "Exercised";
            }

            return ActionAVReview(vm.Reviews, nextreview.ReviewType, nextreview.Guid, contract.Terms, contract.CurrencyID, vm.Templates, nextreview.ReviewDate, contract.IsInHoldOver, (vm is VMSubContractEditModel) ? (vm as VMSubContractEditModel).ParentContracts : null, vm.AssetSchedule, vm.VendorID, IsSubjectToLeaseAccounting, false);
        }

        /// <summary>
        /// Save the and sync actioned AV review.
        /// </summary>
        /// <param name="contractID">The contractID.</param>
        /// <param name="nextreviewid">The nextreviewid.</param>
        /// <param name="actionedreview">The actionedreview.</param>
        /// <param name="UnchangedCosts">The UnchangedCosts.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult SaveAndSyncActionedAVReview(int contractID, VMActionAVReviewModel actionedreview, List<VMAgreedValueContractCostEditModel> UnchangedCosts, bool? hasOptions = false)
        {
            SystemContext.AuditLog.AddAuditEntry("Contract", "SaveAndSyncActionedAVReview", "Start", $"Saving actioned AV Review {actionedreview.ReviewID} for {contractID}");
            ViewBag.ContextID = ContextAssetID;
            UnchangedCosts = UnchangedCosts ?? new List<VMAgreedValueContractCostEditModel>();
            AgreedValueContractEditModel contract = contractService.GetContractEdit(contractID, true) as AgreedValueContractEditModel;
            var jsonsettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
            var serializedObject = JsonConvert.SerializeObject(contract, jsonsettings);
            if (contract == null)
            {
                return ExtendedJson(new { success = false, message = "The contract you are trying to edit may have been deleted." }, JsonRequestBehavior.AllowGet);
            }
            if (actionedreview.EffectiveDate == actionedreview.ContractStart)
            {
                return ExtendedJson(new { success = false, messages = new string[] { ReviewStartDateErrorMessage } }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                actionedreview.ActionedCosts_NotInvoiced = JsonConvert.DeserializeObject<List<VMAgreedValueContractCostEditModel>>(Request.Params["Costs"] ?? "[]", new LocalizedDateTimeJsonConverter());
                actionedreview.RemovedCosts = JsonConvert.DeserializeObject<List<VMAgreedValueContractCostEditModel>>(Request.Params["Removed"] ?? "[]", new LocalizedDateTimeJsonConverter());
                SystemContext.AuditLog.AddAuditEntry("Contract", "SaveAndSyncActionedAVReview", "Checking Templates", $"Processing {actionedreview.Templates.Count} templates in submitted actioned review");
                actionedreview.Templates.ForEach(t =>
                {
                    t.ActionedCosts = JsonConvert.DeserializeObject<List<VMAgreedValueContractCostEditModel>>(Request.Params["Templates[" + t.Guid + "].ActionedCosts"] ?? "[]", new LocalizedDateTimeJsonConverter());
                    t.UnchangedCosts = JsonConvert.DeserializeObject<List<VMAgreedValueContractCostEditModel>>(Request.Params["Templates[" + t.Guid + "].UnchangedCosts"] ?? "[]", new LocalizedDateTimeJsonConverter());
                    t.ActionedCosts = t.ActionedCosts.ToList();
                    t.UnchangedCosts = t.UnchangedCosts.ToList();
                });


                //check if the exercise option date is null maybe bool
                //if it isnt then you set the next option to be exercised
                actionedreview.UnchangedCosts = UnchangedCosts;
                SystemContext.AuditLog.AddAuditEntry("Contract", "SaveAndSyncActionedAVReview", "Checking Templates", "Validating submitted actioned review");

                if (TryValidateModel(actionedreview))
                {
                    var review = contract.NextReview();
                    VMAgreedValueContractEditModel vm = MapAgreedValueContractToVM(contract);
                    VMAgreedValueReviewEditModel lastActioned = vm.Reviews.Where(r => r.ActionedReview != null)
                       .OrderBy(r => r.ActionedReview.EffectiveDate).ThenBy(r => r.ActionedReview.Priority)
                       .Last(r => r.ReviewType != "Adjustment");

                    TermEditModel nextoption;
                    bool exercisedOption = false;
                    if (hasOptions.HasValue && hasOptions.Value == true)
                    {
                        nextoption = contract.NextOption();
                        if (nextoption == null)
                        {
                            return ExtendedJson(new { success = false, message = "Couldn't find the next option to exercise" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            nextoption.State = "Exercised";
                            exercisedOption = true;
                        }
                        contract.Options().FirstOrDefault(t => t.State != "Exercised");
                    }

                    VMAgreedValueReviewEditModel nextreview = vm.Reviews.SingleOrDefault(r => r.ReviewID == review.ReviewID);

                    nextreview.ActionedReview = actionedreview;
                    nextreview.State = "Actioned";

                    TemplateUpdateResult updateresults = SaveAVContract(vm, $"Action Review on {nextreview.ReviewDate.ToString(UserContext.Current.DateFormat)} and attempting to sync to Lease Accelerator");
                    if (ModelState.IsValid)
                    {
                        if (contract.SubjectToLeaseAccounting)
                        {
                            ILeaseAccountingService leaseAccountingService = ServiceLocator.Current.GetInstance<ILeaseAccountingService>();
                            LeaseAccountingReviewEditModel prior = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewForContract(contractID, true);
                            //Sync if 
                            //1. LeaseAccelerator
                            //2. Has a submitted/approved review
                            //3. SaveAVContract was successful

                            //if it's submitted we want to create a new up to date lease accounting review
                            if (prior != null && updateresults != null)
                            {
                                //get the newly updated contract and create a lease accounting review
                                AgreedValueContractEditModel contract1 = contractService.GetContractEdit(contractID) as AgreedValueContractEditModel;
                                LeaseAccountingReviewEditModel draft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract1, false, true);

                                List<string> validationErrors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(draft, contract1, new ValidationContext(draft)).Select(e => e.ErrorMessage).ToList();
                                if (validationErrors.Count > 0)
                                {
                                    throw new DomainValidationException(string.Join(", ", validationErrors.Distinct()));
                                }

                                draft.IsFormalLeaseAccountingReview = true;
                                leaseAccountingService.SetLeaseAccountingReviewState(draft, "Submitted", exercisedOption ? LeaseAccountingReview_ProcessCode.EXERCISE_OPTION : LeaseAccountingReview_ProcessCode.ACTION_REVIEW);

                                return ExtendedJson(new { success = true });
                            }
                        }
                        return ExtendedJson(new { success = true });
                    }
                }
                return ExtendedJson(new { success = false, messages = ModelState.SelectMany(ms => ms.Value.Errors).Select(e => e.ErrorMessage) });
            }
            catch (LeaseAcceleratorImportValidationException leImportValidationException)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(leImportValidationException);
                var original = JsonConvert.DeserializeObject<AgreedValueContractEditModel>(serializedObject, jsonsettings);
                contractService.UpdateContract(original, "Reverting changes because something went wrong with the update. " + leImportValidationException.Message + "|StackTrace:" + leImportValidationException.StackTrace);
                return ExtendedJson(new { success = false, messages = new string[] { leImportValidationException.Message } });
            }
            catch (Exception ex)
            {
                EventLogHelper.LogException("Failed to Save and Sync Actioned AV Review, Attempting to Revert", ex);
                var original = JsonConvert.DeserializeObject<AgreedValueContractEditModel>(serializedObject, jsonsettings);
                contractService.UpdateContract(original, "Reverting changes because something went wrong with the update. " + ex.Message + "|StackTrace:" + ex.StackTrace);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return ExtendedJson(new { success = false, messages = new string[] { ex.Message } });
            }
        }

        /// <summary>
        /// The SaveAndSyncExercisedOption.
        /// </summary>
        /// <param name="contractID">The contractID<see cref="int"/>.</param>
        /// <param name="TermStart">The TermStart<see cref="DateTime"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult SaveAndSyncExercisedOption(int contractID, DateTime TermStart)
        {
            ViewBag.ContextID = ContextAssetID;
            AgreedValueContractEditModel contract = contractService.GetContractEdit(contractID, true) as AgreedValueContractEditModel;
            var jsonsettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
            var serializedObject = JsonConvert.SerializeObject(contract, jsonsettings);

            //AgreedValueContractEditModel original = contractService.GetContractEdit(contractID) as AgreedValueContractEditModel;
            if (contract == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    messages = new string[] { "The contract you are trying to edit may have been deleted." }
                });
            }
            try
            {
                TermEditModel nextoption = contract.NextOption();
                if (nextoption != null)
                    nextoption.State = "Exercised";
                //check if there is a template on the last actioned review
                var lastactionedReview = contract.Reviews.Where(r => r.ActionedReview != null).OrderBy(r => r.ActionedReview.EffectiveDate).ThenBy(r => r.ActionedReview.Priority).Last();
                var costids = lastactionedReview.ActionedReview.Costs.Where(r => r.TemplateCostID.HasValue).Select(a => a.TemplateCostID);

                contract.Templates.Where(r => r.Costs.Any(c => costids.Contains(c.TemplateCostId))).ToList().ForEach(c => c.EndDate = nextoption.TermEnd);


                TemplateUpdateResult updateresults = contractService.UpdateContract(contract, $"cc option: Starting {nextoption.TermStart}");

                if (contract.SubjectToLeaseAccounting)
                {
                    LeaseAccountingReviewEditModel prior = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewForContract(contractID, true);
                    //if it's submitted we want to create a new up to date lease accounting review
                    if (prior != null)
                    {
                        LeaseAccountingReviewEditModel draft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, true);

                        List<string> validationErrors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(draft, contract, new ValidationContext(draft)).Select(e => e.ErrorMessage).ToList();
                        if (validationErrors.Count > 0)
                        {
                            throw new DomainValidationException(string.Join(", ", validationErrors.Distinct()));
                        }

                        //exercising an option requires a formal lease accounting review
                        draft.IsFormalLeaseAccountingReview = true;
                        leaseAccountingService.UpdateLeaseAccountingReview(draft);
                        leaseAccountingService.SetLeaseAccountingReviewState(draft, "Submitted", LeaseAccountingReview_ProcessCode.EXERCISE_OPTION);
                        draft = leaseAccountingService.GetLeaseAccountingReviewEdit(draft.LeaseAccountingReviewID);
                    }
                }

                return ExtendedJson(new
                {
                    success = true,
                    invoicesRemoved = updateresults.UnsubmittedInvoicesRemoved.Count,
                    batchesRemovedFrom = updateresults.UnsubmittedInvoicesRemoved.Select(i => i.BatchID).Distinct().Count(),
                    submittedInvoicesRetained = updateresults.SubmittedInvoicesRetained.Count
                });
            }
            catch (LeaseAcceleratorImportValidationException leImportValidationException)
            {
                var original = JsonConvert.DeserializeObject<AgreedValueContractEditModel>(serializedObject, jsonsettings);
                contractService.UpdateContract(original, "Reverting changes because something went wrong with the update. " + leImportValidationException.Message + "|StackTrace:" + leImportValidationException.StackTrace);
                return ExtendedJson(new { success = false, messages = new string[] { leImportValidationException.Message } });
            }
            catch (DomainValidationException ex)
            {
                var original = JsonConvert.DeserializeObject<AgreedValueContractEditModel>(serializedObject, jsonsettings);

                contractService.UpdateContract(original, "Reverting changes because something went wrong with the update. " + ex.Message + "|StackTrace:" + ex.StackTrace);
                EventLogHelper.LogException("Failed to Save and Sync Exercising Option, Attempting to Revert", ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return ExtendedJson(new { success = false, messages = ex.Errors.Select(e => e.Message) });
            }
            catch (Exception ex)
            {
                var original = JsonConvert.DeserializeObject<AgreedValueContractEditModel>(serializedObject, jsonsettings);
                contractService.UpdateContract(original, "Reverting changes because something went wrong with the update. " + ex.Message + "|StackTrace:" + ex.StackTrace);
                EventLogHelper.LogException("Failed to Save and Sync Exercising Option, Attempting to Revert", ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return ExtendedJson(new { success = false, messages = new string[] { ex.Message } });
            }
        }

        /// <summary>
        /// Search the commencing review.
        /// </summary>
        /// <param name="contractID">The contractID.</param>
        /// <param name="TermStart">The TermStart.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult SearchCommencingReview(int contractID, DateTime TermStart)
        {
            AgreedValueContractEditModel original = contractService.GetContractEdit(contractID) as AgreedValueContractEditModel;
            if (original == null)
            {
                return ExtendedJson(new { success = false, message = "An error occurred, contract not found" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                VMAgreedValueContractEditModel vm = MapAgreedValueContractToVM(original);
                VMAgreedValueReviewEditModel firstNotActioned = vm.Reviews.Where(r => r.State == "Pending"
                && r.ReviewDate.Month == TermStart.Month && r.ReviewDate.Year == TermStart.Year).FirstOrDefault();

                var reviewsPendingBeforeFirstNotActioned = vm.Reviews.Where(c => c.ReviewDate.Date < TermStart.Date && c.State == "Pending").ToList();
                if (firstNotActioned != null && reviewsPendingBeforeFirstNotActioned.Count > 0)
                {
                    return ExtendedJson(new { Id = 0, success = false, message = "There is an unactioned review prior to the option " + TermStart.Date.ToShortDateString() + " Please action all prior reviews before exercising the option." }, JsonRequestBehavior.DenyGet);
                }
                else if (firstNotActioned != null)
                {
                    return ExtendedJson(new
                    {
                        Id = firstNotActioned.ReviewID,
                        success = true,
                        html = RenderVariantPartialViewToString("Dialog/CommencingReview", firstNotActioned),

                    });
                }
                else
                {
                    return ExtendedJson(new
                    {
                        Id = 0,
                        messages = "No Reviews found",
                        success = true,

                    });
                }
            }
        }


        /// <summary>
        /// Action an RB review (select assets and set metric/rates.
        /// </summary>
        /// <param name="terms">     list of terms currently on the contract.</param>
        /// <param name="reviews">   list of reviews currently on the contract.</param>
        /// <param name="guid">      guid of the review being saved.</param>
        /// <param name="currencyID">currency ID currently selected on contract.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult ActionRBReview(List<TermEditModel> terms, List<VMRateReviewEditModel> reviews, string guid, int currencyID)
        {
            ModelState.Clear();
            ViewBag.CurrencyFormat = localeService.GetCurrency(currencyID).FormatString;
            reviews = (reviews ?? new List<VMRateReviewEditModel>()).OrderBy(r => r.ReviewDate).ToList();
            terms = (terms ?? new List<TermEditModel>()).OrderBy(t => t.TermStart).ToList();

            if (terms.Count < 1)
            {
                return ExtendedJson(new { success = false, message = "An initial term must be added to the contract before costs and reviews can be defined" });
            }

            VMRateReviewEditModel review = reviews.SingleOrDefault(r => r.Guid == guid);
            if (review == null)
            {
                if (reviews.Count < 1)
                {
                    review = new VMRateReviewEditModel
                    {
                        ActionedReview = new RateActionedReviewEditModel
                        {
                            Assets = new List<int> {
                                ContextAssetID
                            },
                            ChargeRates = new List<ChargeRateEditModel>
                            {
                                new ChargeRateEditModel {
                                    ChargeRate = 0,
                                    Metric = ""
                                }
                            },
                            ActionedDate = terms[0].TermStart,
                            EffectiveDate = terms[0].TermStart
                        },
                        Guid = "costs",
                        ReviewDate = terms[0].TermStart
                    };
                }
                else
                {
                    return ExtendedJson(new { success = false, message = "The review does not exist and cannot be edited. Please try again" });
                }
            }
            if (reviews.Count > 0)
            {
                VMRateReviewEditModel last = reviews.Where(r => r.ActionedReview != null).OrderBy(r => r.ActionedReview.EffectiveDate).Last();
                if (review.ActionedReview == null)
                {
                    review.ActionedReview = new RateActionedReviewEditModel
                    {
                        ActionedDate = DateTime.Now,
                        Assets = last.ActionedReview.Assets,
                        EffectiveDate = review.ReviewDate,
                        ChargeRates = last.ActionedReview.ChargeRates
                    };
                }
            }
            ViewBag.AssetID = ContextAssetID;
            ViewBag.AssetNames = assetService.GetAssetSelectList(currencyID).ToDictionary(a => int.Parse(a.Key), a => a.Name);
            review.State = "Actioned";
            return ExtendedJson(new
            {
                success = true,
                html = RenderVariantPartialViewToString("Partial/ActionRVReview", review),
                rows = review.ActionedReview.ChargeRates,
                metrics = contractService.GetAllInUseMetricTypes()
                    .Union(reviews.Where(r => r.ActionedReview != null).SelectMany(c => c.ActionedReview.ChargeRates.Select(r => r.Metric)))
                    .Union(review.ActionedReview.ChargeRates.Select(c => c.Metric))
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList()
            });
        }

        /// <summary>
        /// The AddEditClauseAmendment.
        /// </summary>
        /// <param name="clauseText">The clauseText<see cref="string"/>.</param>
        /// <param name="amendments">The amendments<see cref="List{ContractClauseAmendmentEditModel}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AddEditClauseAmendment(string clauseText, List<ContractClauseAmendmentEditModel> amendments)
        {
            //we dont want validation going off
            ModelState.Clear();
            ContractClauseAmendmentEditModel model = amendments?.FirstOrDefault() ?? new ContractClauseAmendmentEditModel { Amendment = clauseText };
            ViewBag.ClauseText = clauseText;
            return PartialView("EditorTemplates/ContractClauseAmendmentEditModel", model);
        }

        /// <summary>
        /// The AddEditClauseTriggeredRecord.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="contractClauseId">The contractClauseId<see cref="int"/>.</param>
        /// <param name="triggeredRecords">The triggeredRecords<see cref="List{ClauseTriggeredRecordEditModel}"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult AddEditClauseTriggeredRecord(int id, int contractClauseId, List<ClauseTriggeredRecordEditModel> triggeredRecords)
        {
            ClauseTriggeredRecordEditModel model = triggeredRecords?.FirstOrDefault() ?? new ClauseTriggeredRecordEditModel
            {
                ContractClauseID = contractClauseId,
                RecordID = -1,
                TriggeredOn = DateTime.Today
            };
            ModelState.Clear();
            return PartialView("EditorTemplates/ClauseTriggeredRecordEditModel", model);
        }

        /// <summary>
        /// The AddEditExitCost.
        /// </summary>
        /// <param name="exitcosts">The exitcosts<see cref="List{ExitCostEditModel}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AddEditExitCost(List<ExitCostEditModel> exitcosts)
        {
            ModelState.Clear();
            exitcosts = exitcosts ?? new List<ExitCostEditModel>();
            if (exitcosts.Count < 1)
            {
                exitcosts.Add(new ExitCostEditModel
                {
                    Amount = 0M,
                    Description = "",
                    ID = -1
                });
            }
            return PartialView("EditorTemplates/ExitCostEditModel", exitcosts[0]);
        }

        /// <summary>
        /// The AddEditIncentive.
        /// </summary>
        /// <param name="incentives">The incentives<see cref="List{IncentiveEditModel}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AddEditIncentive(List<IncentiveEditModel> incentives)
        {
            ModelState.Clear();
            incentives = incentives ?? new List<IncentiveEditModel>();
            if (incentives.Count < 1)
            {
                incentives.Add(new IncentiveEditModel
                {
                    Amount = 0M,
                    Date = DateTime.Today,
                    Description = "",
                    ID = -1,
                    Type = ""
                });
            }
            return PartialView("EditorTemplates/IncentiveEditModel", incentives[0]);
        }

        /// <summary>
        /// The AddEditInitialCost.
        /// </summary>
        /// <param name="initialcosts">The initialcosts<see cref="List{InitialCostEditModel}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AddEditInitialCost(List<InitialCostEditModel> initialcosts)
        {
            ModelState.Clear();
            initialcosts = initialcosts ?? new List<InitialCostEditModel>();
            if (initialcosts.Count < 1)
            {
                initialcosts.Add(new InitialCostEditModel
                {
                    Amount = 0M,
                    Description = "",
                    ID = -1
                });
            }
            return PartialView("EditorTemplates/InitialCostEditModel", initialcosts[0]);
        }

        /// <summary>
        /// The AddEditMakeGoodCost.
        /// </summary>
        /// <param name="makegoodcosts">The makegoodcosts<see cref="List{MakeGoodCostEditModel}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AddEditMakeGoodCost(List<MakeGoodCostEditModel> makegoodcosts)
        {
            ModelState.Clear();
            makegoodcosts = makegoodcosts ?? new List<MakeGoodCostEditModel>();
            if (makegoodcosts.Count < 1)
            {
                makegoodcosts.Add(new MakeGoodCostEditModel
                {
                    Amount = 0M,
                    Type = "",
                    Description = "",
                    ID = -1
                });
            }
            return PartialView("EditorTemplates/MakeGoodCostEditModel", makegoodcosts[0]);
        }

        /// <summary>
        /// The AddEditSubcontractMappings.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="subcontractID">The subcontractID<see cref="int?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult AddEditSubcontractMappings(int ID, int? subcontractID)
        {
            ViewBag.ContextID = ContextAssetID;
            CreateSubContractModel model = new CreateSubContractModel();
            VMParentContractsModel mapping = new VMParentContractsModel();
            AgreedValueContractViewModel ParentContract = contractService.GetContractView(ID) as AgreedValueContractViewModel;

            IEnumerable<AssetViewModel> allAssets = assetService.FindMatchingAssets("", null, status: assetService.GetAssetStatuses().ToArray()).Select(a => SimpleMapper.Map<AssetListModel, AssetViewModel>(a));
            Dictionary<int, AssetViewModel> Assets = allAssets
                .Where(a => ParentContract.Assets().Contains(a.AssetID)).ToDictionary(a => a.AssetID, a => a);
            Dictionary<int, IGrouping<int, AssetViewModel>> ChildAssets = allAssets.Where(a => a.ParentID.HasValue && ParentContract.Assets().Contains(a.ParentID.Value))
                .GroupBy(a => a.ParentID.Value, a => a)
                .ToDictionary(a => a.Key, a => a);
            //mapping.ParentContractID =
            mapping.SubContractMappings = Assets.Select((a, i) => new VMSubContractMappingModel
            {
                ParentAsset = a.Value,
                ParentAssetID = a.Key,
                ExistingChildAssets = ChildAssets.ContainsKey(a.Key) ? ChildAssets[a.Key].ToList() : new List<AssetViewModel>(),
                Asset = a.Value,
                AssetID = a.Key,
                ParentContractID = ID,
                ParentContract = ParentContract,
                ChildAssetDetails = new ChildAssetEditModel
                {
                    ID = -1 * i,
                    Ownership = "OTHER"
                }
            }).ToList();
            if (subcontractID.HasValue)
            {
                AgreedValueContractEditModel subcontract = contractService.GetContractEdit(subcontractID.Value) as AgreedValueContractEditModel;
                foreach (VMSubContractMappingModel map in mapping.SubContractMappings)
                {
                    int tempid = -1;
                    SubContractMappingEditModel match = subcontract.ParentContracts.Find(a => a.ParentContractID == map.ParentContractID
                        && (map.Asset.ParentID == a.AssetID || map.AssetID == a.AssetID));
                    map.Percentage = match.Percentage;
                    if (match.AssetID != map.AssetID)
                    {
                        map.ChildAssetDetails = new ChildAssetEditModel
                        {
                            Name = match.Asset.Name,
                            AssetType = match.Asset.AssetTypeID,
                            BusinessUnit = match.Asset.BusinessUnit,
                            LegalEntity = match.Asset.LegalEntity,
                            Ownership = match.Asset.Ownership,
                            ID = tempid--
                        };
                    }
                }
            }
            model.ParentContracts = new List<VMParentContractsModel> { mapping };

            return PartialView("Partial/SubContracts/CreateSubContract", model);
        }

        /// <summary>
        /// The AddSubContractMappingToSubContract.
        /// </summary>
        /// <param name="subcontractid">The subcontractid<see cref="int"/>.</param>
        /// <param name="contractid">The contractid<see cref="int"/>.</param>
        /// <param name="BaseAssetID">The BaseAssetID<see cref="int?"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult AddSubContractMappingToSubContract(int subcontractid, int contractid, int? BaseAssetID)
        {
            AgreedValueContractViewModel parentcontract = contractService.GetContractView(contractid) as AgreedValueContractViewModel;
            AgreedValueContractViewModel subcontract = contractService.GetContractView(subcontractid) as AgreedValueContractViewModel;
            AddNewContractMappingViewModel model = new AddNewContractMappingViewModel
            {
                Mapping = new VMSubContractMappingModel
                {
                    ParentContractID = contractid,
                    ParentContract = parentcontract,
                    ContractID = subcontractid,
                    Contract = subcontract
                }
            };

            List<AssetListModel> assets = assetService.FindMatchingAssets("", null);

            model.ParentAssets = assets.Where(a => parentcontract.Assets().Contains(a.AssetID)).ToList();
            if (BaseAssetID.HasValue)
            {
                model.ParentAssets = model.ParentAssets.Where(a => a.AssetID == BaseAssetID.Value).ToList();
            }
            model.ExistingChildAssets = new Dictionary<int, List<AssetListModel>>();
            model.ChildAsset = new ChildAssetEditModel();
            //model.
            foreach (AssetListModel asset in model.ParentAssets)
            {
                model.ExistingChildAssets.Add(asset.AssetID, assets.Where(a => a.ParentID == asset.AssetID).ToList());
            }
            return PartialView("Partial/SubContracts/AddSubContractMappingToSubContract", model);
        }

        /// <summary>
        /// Start the Vary Lease process
        /// </summary>
        /// <param name="id">The contract id</param>
        /// <returns>The partial view. <see cref="PartialViewResult"/></returns>
        public PartialViewResult BeginVaryLease(int id)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return PartialUnauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return PartialUnauthorized();
            }

            AgreedValueContractEditModel contract = contractService.GetContractEdit(id, false) as AgreedValueContractEditModel;
            if (contract == null)
            {
                return PartialView("Partial/Error", new { message = "The contract you're trying to vary could not be found and may have been removed by another user." });
            }
            if (contract.IsArchived)
            {
                return PartialView("Partial/Error", new { message = "The contract you're trying to vary is archived and cannot be modified." });
            }
            if (!contract.SubjectToLeaseAccounting)
            {
                return PartialView("Partial/Error", new { message = "The contract you're trying to vary is not subject to lease accounting." });
            }

            var priorLeaseAccountingReviews = leaseAccountingService.GetPriorLeaseAccountingReviews(contract.ContractID, TimeSpan.MaxValue).ToList();
            var data = new DateOfModification { ValidDateList = GetValidModificationDates(contract, priorLeaseAccountingReviews) };

            ViewBag.PaymentMode = contract.Reviews.First(r => r.ReviewType == "Commencing").ActionedReview.Costs.First().PaidInArrears ? "In Arrears" : "In Advance";
            return PartialView("Dialog/BeginVaryLease", data);
        }

        /// <summary>
        /// Start update Reasonable Certainty process
        /// </summary>
        /// <param name="id">The contract id</param>
        /// <returns>The partial view. <see cref="PartialViewResult"/></returns>
        public PartialViewResult BeginUpdateRC(int id)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return PartialUnauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return PartialUnauthorized();
            }

            AgreedValueContractEditModel contract = contractService.GetContractEdit(id, false) as AgreedValueContractEditModel;
            if (contract == null)
            {
                return PartialView("Partial/Error", new { message = "The contract you're trying to update reasonable certainty could not be found and may have been removed by another user." });
            }
            if (contract.IsArchived)
            {
                return PartialView("Partial/Error", new { message = "The contract you're trying to update reasonable certainty is archived and cannot be modified." });
            }
            if (!contract.SubjectToLeaseAccounting)
            {
                return PartialView("Partial/Error", new { message = "The contract you're trying to update reasonable certainty is not subject to lease accounting." });
            }

            var priorLeaseAccountingReviews = leaseAccountingService.GetPriorLeaseAccountingReviews(contract.ContractID, TimeSpan.MaxValue).ToList();
            var data = new DateOfModification { ValidDateList = GetValidModificationDates(contract, priorLeaseAccountingReviews) };

            return PartialView("Dialog/BeginUpdateRC", data);
        }

        /// <summary>
        /// Validate the modification date falls within the life of the contract
        /// </summary>
        /// <param name="id">The contract id</param>
        /// <param name="effectiveDate">date formatted according to user settings</param>
        /// <returns></returns>
        public ExtendedJsonResult ValidateModificationDate(int id, string effectiveDate, string action)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return JsonUnauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return JsonUnauthorized();
            }

            AgreedValueContractEditModel contract = contractService.GetContractEdit(id, false) as AgreedValueContractEditModel;
            if (contract == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The contract you're trying to " + action + " could not be found and may have been removed by another user."
                }, JsonRequestBehavior.AllowGet);
            }
            if (contract.IsArchived)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The contract you're trying to " + action + " is archived and cannot be modified."
                }, JsonRequestBehavior.AllowGet);
            }
            if (!contract.SubjectToLeaseAccounting)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The contract you're trying to " + action + " is not subject to lease accounting."
                }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrWhiteSpace(effectiveDate))
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The effective date must be selected."
                }, JsonRequestBehavior.AllowGet);
            }
            // If effective date (event start date) and LeaseAccounting_StartDate (contract start date) are the same should show the validation message
            DateTime date = DateTime.ParseExact(effectiveDate, UserContext.Current.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
            if (!string.IsNullOrWhiteSpace(effectiveDate) && date.Equals(contract.LeaseAccounting_StartDate))
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "Date error. The event start date cannot be the same as the Contract start date. Please adjust the dates and resubmit."
                }, JsonRequestBehavior.AllowGet);
            }

            if (DateTime.TryParseExact(effectiveDate, UserContext.Current.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out DateTime _))
            {
                return ExtendedJson(new
                {
                    success = true,
                    date = effectiveDate
                }, JsonRequestBehavior.AllowGet);
            }
            return ExtendedJson(new
            {
                success = false,
                message = "The selected effective date could not be validated. Please reselect the date and try again."
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The ContractAuditLog.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="fromDate">The fromDate<see cref="DateTime?"/>.</param>
        /// <param name="toDate">The toDate<see cref="DateTime?"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult ContractAuditLog(int ID, DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Group by changeset
            List<IGrouping<Guid, SystemAuditLogEntry>> groupedAuditLogs = new List<IGrouping<Guid, SystemAuditLogEntry>>();

            if (fromDate != null && toDate != null)
            {
                groupedAuditLogs = auditService.GetContractsLeaseAccountingReviewAuditEntries(new List<int> { ID }, fromDate, toDate).OrderBy(c => c.EntryDateTime).ThenBy(c=>c.ID).GroupBy(al => al.ChangeSet).Select(al => al).ToList();
            }

            VMContractAuditLog auditLogVM = new VMContractAuditLog
            {
                contractAuditLogEntries = groupedAuditLogs,
                fromDate = fromDate,
                toDate = toDate
            };

            return PartialView("Tabs/ContractAuditLog", auditLogVM);
        }

        /// <summary>
        /// The ContractClauseAmendment.
        /// </summary>
        /// <param name="model">The amendment <see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ContractClauseAmendment(ContractClauseAmendmentEditModel model)
        {
            if (ModelState.IsValid)
            {
                return ExtendedJson(new { success = true, html = RenderVariantPartialViewToString("DisplayTemplates/ContractClauseAmendmentEditModel", model) });
            }
            return PartialView("EditorTemplates/ContractClauseAmendmentEditModel", model);
        }

        /// <summary>
        /// The ContractDetails.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult ContractDetails(int ID)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Base))
            {
                return PartialUnauthorized();
            }

            ContractViewModel contract = contractService.GetContractView(ID);
            ContractDetailsViewModel vm = new ContractDetailsViewModel
            {
                Contract = contract,
                AssetIsEditable = assetService.AssetIsEditable(ContextAssetID) && UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit),
                ContextAsset = assetService.GetAssetList(ContextAssetID)
            };
            var assetsids = contract.Assets().ToList().Union(new List<int> { ContextAssetID });
            Dictionary<int, string> assets = assetService.GetAssetSelectList().Where(r => assetsids.Select(a => a.ToString()).Contains(r.Key)).ToDictionary(a => int.Parse(a.Key), a => a.Name);

            vm.Assets = assets;
            FileCollectionEditModel collection = fileService.GetFileCollection(contract.EntityID, "DocumentTemplates");
            if (collection == null)
            {
                fileService.CreateFileCollection(new FileCollectionEditModel
                {
                    CollectionID = -1,
                    CollectionKey = "DocumentTemplates",
                    Description = "Documents Templates for Contract " + contract.Description,
                    EntityType = "Contract",
                    EntityID = contract.EntityID
                });
            }
            else
            {
                collection.Description = "Documents Templates for Contract " + contract.Description;
                fileService.UpdateFileCollectionName(collection);
            }

            vm.IsLeaseAccountingEnabledForContract = ClientContext.Current.GetConfigurationSetting("LeaseAccelerator.Synchronsiation", true) && contract.SubjectToLeaseAccounting;
            var status = new List<LeaseAccountingSyncStatusModel>();
            if (vm.IsLeaseAccountingEnabledForContract && contract is AgreedValueContractViewModel)
            {
                AgreedValueContractEditModel avcontract = contractService.GetContractEdit(contract.ContractID, false) as AgreedValueContractEditModel;
                status = leaseAccountingService.GetLeaseAccountingReviewSynchronisationStatusByContract(contract.ContractID);

                vm.IsLeaseAccountingEnabledForContract = LeaseAccountingProviderFactory.Current.IsContractLeaseAccountingEnabled(avcontract, null, true, null);
                if (vm.IsLeaseAccountingEnabledForContract)
                {
                    if (LoisProvider.IsEnabled)
                    {
                        vm.LeaseAccountingLedgerSystems = leaseAccountingService.GetLedgerSystems();
                    }
                }
                var currentdraft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(avcontract, false, false);
                vm.ValidateReview(avcontract, currentdraft);
            }
            vm.CreateContractActionItems(status);

            ViewBag.SubjectToLeaseAccounting = contract.SubjectToLeaseAccounting;

            contract.CustomFieldValues.Where(r => r.EntityID != contract.EntityID && r.CustomField.MinimumValues < 1).Select(c => { c.Value = ""; return c; }).ToList();
            return PartialView("Tabs/ContractDetails", vm);
        }

        /// <summary>
        /// The ContractInvoices.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult ContractInvoices(int ID)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Base))
            {
                return PartialUnauthorized();
            }

            ContractViewModel contract = contractService.GetContractView(ID);
            ViewBag.AssetID = ContextAssetID;
            ViewBag.AssetIsEditable = assetService.AssetIsEditable(ContextAssetID) && UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit);
            ViewBag.Currency = new CurrencyViewModel { CurrencyID = contract.CurrencyID, FormatString = contract.CurrencyFormat, Name = contract.Currency };

            if (contract.SubjectToLeaseAccounting)
            {
                List<LeaseAccountingSyncStatusModel> LeaseAccountingSyncStatus =
                    leaseAccountingService.GetLeaseAccountingReviewSynchronisationStatusByContract(contract.ContractID);

                if (LeaseAccountingSyncStatus.Count > 0)
                {
                    ViewBag.HideEdit = LeaseAccountingSyncStatus.Any(r => r.LAP_EventCode != LeaseAccountingReview_EventCode.ACCT_APPROVED.ToString() || r.LAP_ProcessCode == LeaseAccountingReview_ProcessCode.ROLLBACK.ToString());
                    ViewBag.HideDelete = ViewBag.HideEdit;
                }
            }

            return PartialView("Tabs/ContractInvoices", contract);
        }

        /// <summary>
        /// The CreateAgreedValueContract.
        /// </summary>
        /// <param name="createModel">The createModel<see cref="CreateContractModel"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult CreateAgreedValueContract(CreateContractModel createModel)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return PartialUnauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return PartialUnauthorized();
            }

            AssetViewModel asset = assetService.GetAssetView(ContextAssetID, false, false, false, false, false);
            ContractTypeEditModel ct = contractTypeService.GetContractType(createModel.ContractTypeID);
            ViewBag.SubjectToLeaseAccounting = ct.SubjectToLeaseAccounting;
            //sanity check for subject to lease accounting
            if (asset.Ownership.ToUpper() != "LEASED" && ct.SubjectToLeaseAccounting)
            {
                //TODO return proper error message
                return PartialUnauthorized();
            }
            VMAgreedValueContractEditModel model = new VMAgreedValueContractEditModel
            {
                IsReceivable = createModel.IsReceivable
            };
            model.ContractTypeID = ct.ContractTypeID;
            model.ContractType = ct.Name;
            model.ContractCategory = ct.Category;
            model.Terms = new List<TermEditModel> {
                new VMTermEditModel {
                    TermStart = createModel.ContractStart ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    TermEnd = createModel.ContractEnd ?? (createModel.ContractStart ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)).AddYears(1).AddDays(-1),
                    State = "Exercised",
                    IsOption = false,
                    TermName = "Initial Term"
                }
            };
            model.MakeGoodDateOfObligation = model.Terms.First(c => !c.IsOption).TermEnd.Value;
            if (asset.Address != null)
            {
                model.CurrencyID = localeService.GetCountryByID(asset.Address.CountryID).DefaultCurrency.CurrencyID;
            }
            model.CustomFieldValues = ct.CustomFields.Where(cf => cf.EntitySubType == (model.IsReceivable ? "Receivable" : "Payable"))
                .SelectMany(c => c.Mappings.Select(m =>
                    m.DefaultValue ?? MappingContext.Instance.Map<CustomFieldEditModel, CustomFieldValueEditModel>(m.CustomField)
                 )).ToList();
            model.SubjectToLeaseAccounting = ct.SubjectToLeaseAccounting;
            if (ct.SubjectToLeaseAccounting)
            {
                AssetEditModel aEditModel = assetService.GetAssetEdit(ContextAssetID);
                model.AssetSchedule.Add(new ContractAssetScheduleItemEditModel
                {
                    ID = -1,
                    Asset = asset.FullName,
                    IsPrimaryAsset = true,
                    AssetID = ContextAssetID,
                    BusinessUnit = aEditModel.BusinessUnit,
                    BusinessUnitID = aEditModel.BusinessUnitID,
                    LegalEntity = aEditModel.LegalEntity,
                    LegalEntityID = aEditModel.LegalEntityID,
                });
                model.IsPartialBuilding = true;
            }
            int id = 0;
            model.OtherClauses.AddRange(ct.PredefinedClauses.Where(c => !model.OtherClauses.Any(c2 => c2.Category == c.Category && c2.Clause == c.Clause)).Select(c => new ContractClauseEditModel
            {
                Clause = c.Clause,
                Category = c.Category,
                ClauseText = "",
                IsActive = c.IsRequired,
                IsRequired = c.IsRequired,
                IsPredefinedClause = true,
                ContractClauseID = --id,
                ContractID = model.ContractID
            }));
            SetupEditViewBag(model);
            return PartialView("EditorTemplates/ContractEditmodel", model);
        }

        /// <summary>
        /// Create a new review for an agreed value contract.
        /// </summary>
        /// <param name="type">      Fixed, CPI, Market</param>
        /// <param name="terms">     a list of all the contract terms currently in the contract</param>
        /// <param name="currencyID">the currently selected currency id</param>
        /// <param name="reviews">   a list of all the reviews currently in the contract</param>
        /// <param name="ParentContracts">The ParentContracts<see cref="List{VMParentContractsModel}"/></param>
        /// <param name="AssetSchedule">The AssetSchedule<see cref="List{ContractAssetScheduleItemEditModel}"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        [HttpPost]
        public ExtendedJsonResult CreateAVReview(string type, List<TermEditModel> terms, int currencyID, List<VMAgreedValueReviewEditModel> reviews, List<VMParentContractsModel> ParentContracts, List<ContractAssetScheduleItemEditModel> AssetSchedule)
        {
            ModelState.Clear();
            ViewBag.CurrencyFormat = localeService.GetCurrency(currencyID).FormatString;
            ViewBag.InvoiceTypes = invoiceTypeService.GetInvoiceTypes().Select(g => new SelectListItem { Text = g.Name, Value = g.InvoiceTypeID.ToString() }).ToList();
            ViewBag.VaryLease = Request["VaryLease"] == "True";
            reviews = (reviews ?? new List<VMAgreedValueReviewEditModel>()).OrderBy(r => r.ReviewDate).ToList();
            terms = (terms ?? new List<TermEditModel>()).OrderBy(t => t.TermStart).ToList();
            List<SelectItem> assetlist = assetService.GetAssetSelectList(currencyID);
            if (ParentContracts != null)
            {
                ParentContracts.SelectMany(pc => pc.SubContractMappings.Select(c => c.ChildAssetDetails)).ToList().ForEach(a =>
                {
                    assetlist.Add(new SelectItem { Key = a.ID.ToString(), Name = a.Name, Visible = true });
                });
            }
            if (AssetSchedule != null)
            {
                AssetSchedule.ToList().ForEach(a =>
                {
                    assetlist.Add(new SelectItem { Key = a.AssetID.ToString(), Name = a.Asset, Visible = true });
                });
            }
            if (terms.Count < 1)
            {
                return ExtendedJson(new { success = false, message = "An initial term must be added to the contract before costs and reviews can be defined" });
            }

            DateTime reviewDate = DateTime.Now;
            if (reviews.Count == 1)
            {
                reviewDate = reviews[0].ReviewDate.AddMonths(1);
            }
            else if (reviews.Count > 1)
            {
                VMAgreedValueReviewEditModel secondLast = reviews[reviews.Count - 2];
                VMAgreedValueReviewEditModel last = reviews[reviews.Count - 1];
                reviewDate = last.ReviewDate.Day == secondLast.ReviewDate.Day ? last.ReviewDate.AddMonths(last.ReviewDate.MonthsBetween(secondLast.ReviewDate)) : last.ReviewDate.AddDays((last.ReviewDate - secondLast.ReviewDate).TotalDays);
            }
            VMAgreedValueReviewEditModel lastActioned = reviews.Last(r => r.ActionedReview != null);
            if (lastActioned == null)
            {
                return ExtendedJson(new { success = false, message = "Commencing costs must be configured before adding reviews" });
            }

            VMAgreedValueReviewEditModel review = new VMAgreedValueReviewEditModel
            {
                ReviewDate = reviewDate,
                ReviewType = type,
                Guid = Guid.NewGuid().ToString(),
                IsNew = true
            };
            IEnumerable<CostCategoryListModel> categories = costCategoryService.GetAllCostCategories();
            Dictionary<string, string> cpiregions = contractService.GetCPIRegionList().ToDictionary(r => r.ID.ToString(), r => r.Name);
            review.Costs = new List<AgreedValueReviewCostEditModel>();

            var combinedCosts = lastActioned.ActionedReview.ActionedCosts_NotInvoiced;
            combinedCosts.AddRange(lastActioned.ActionedReview.Templates.SelectMany(t => t.ActionedCosts));
            combinedCosts.AddRange(lastActioned.ActionedReview.Templates.SelectMany(t => t.UnchangedCosts));
            combinedCosts.AddRange(lastActioned.ActionedReview.UnchangedTemplates.SelectMany(t => t.UnchangedCosts));
            combinedCosts.AddRange(lastActioned.ActionedReview.UnchangedTemplates.SelectMany(t => t.ActionedCosts));

            switch (type)
            {
                case "Fixed":
                    review.Costs = lastActioned.ActionedReview.ActionedCosts_NotInvoiced.Select(c => new AgreedValueReviewCostEditModel
                    {
                        YearlyAmount = c.YearlyAmount,
                        AssetID = c.AssetID,
                        CategoryID = c.CategoryID,
                        Label = c.Label ?? "",
                        PaymentAmount = c.PaymentAmount,
                        PaymentFrequency = c.PaymentFrequency,
                        PaymentPattern = c.PaymentPattern,
                        JurisdictionCode = c.JurisdictionCode,
                        TaxRateID = c.TaxRateID,
                        TaxAmount = c.TaxAmount,
                        CategoryIsLeaseAccountingSignificant = c.CategoryIsLeaseAccountingSignificant
                    }).ToList();
                    break;

                case "Fixed%":
                    review.Costs = lastActioned.ActionedReview.ActionedCosts_NotInvoiced.Select(c => new AgreedValueReviewCostEditModel
                    {
                        AssetID = c.AssetID,
                        CategoryID = c.CategoryID,
                        Label = c.Label ?? "",
                        FixedPercent = 0
                    }).ToList();
                    break;

                case "Market":
                    review.Costs = lastActioned.ActionedReview.ActionedCosts_NotInvoiced.GroupBy(c => c.CategoryID + "|" + c.AssetID).Select(c => new AgreedValueReviewCostEditModel
                    {
                        //Label = c.First().Label ?? "",
                        AssetID = c.First().AssetID,
                        CategoryID = c.First().CategoryID,
                        Cap = 0,
                        Collar = 0,
                        Estimate = 0,
                        Plus = 0
                    }).ToList();
                    break;

                case "CPI":
                    review.Costs = lastActioned.ActionedReview.ActionedCosts_NotInvoiced.GroupBy(c => c.CategoryID + "|" + c.AssetID).Select(c => new AgreedValueReviewCostEditModel
                    {
                        //Label = c.First().Label ?? "",
                        AssetID = c.First().AssetID,
                        CategoryID = c.First().CategoryID,
                        Cap = 0,
                        Collar = 0,
                        Estimate = 0,
                        Plus = 0,
                        CPIRegionID = int.Parse(cpiregions.First().Key)
                    }).ToList();
                    break;

                default:
                    return ExtendedJson(new { success = false, message = "The requested review type does not exist. Please try again" });
            }

            return ExtendedJson(new
            {
                success = true,
                type,
                html = RenderVariantPartialViewToString("EditorTemplates/AgreedValueReviewEditModel", review),
                rows = review.Costs,
                cpiregions,
                categories = categories.ToDictionary(c => c.CostCategoryID.ToString(), c => c.DisplayName()),
                assets = assetlist,
                jurisdictions = localeService.GetTaxJurisdictions().Values.ToDictionary(j => j.Code, j => new
                {
                    code = j.Code,
                    name = j.Name,
                    taxrates = (IList<VMTaxRateViewModel>)null
                })
            });
        }

        /// <summary>
        /// The CreateBreakClause.
        /// </summary>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult CreateBreakClause()
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return JsonUnauthorized();
            }

            return ExtendedJson(new { success = true, html = RenderVariantPartialViewToString("EditorTemplates/BreakClauseEditModel", new BreakClauseEditModel()) }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The CreateClause.
        /// </summary>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult CreateClause()
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return JsonUnauthorized();
            }
            ViewBag.ClauseCategories = GenerateClauseHeirarchy(null);
            return ExtendedJson(new
            {
                success = true,
                html = RenderVariantPartialViewToString("EditorTemplates/ContractClauseEditModel", new ContractClauseEditModel())
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The CreateContract.
        /// </summary>
        /// <param name="model">The model<see cref="CreateContractModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult CreateContract(CreateContractModel model)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return Unauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return Unauthorized();
            }
            ContractTypeEditModel ct = contractTypeService.GetContractType(model.ContractTypeID);
            if (ct.SubjectToLeaseAccounting)
            {
                string validateSchedule = contractService.ValidateAssetScheduleAddition(-1, ContextAssetID, -1);
                if (!string.IsNullOrEmpty(validateSchedule))
                {
                    ModelState.AddModelError("ContractType", validateSchedule);
                }
            }
            if (ModelState.IsValid)
            {
                switch (model.ContractClassification)
                {
                    case "agreedvalue":
                        return ExtendedJson(new { success = true, url = Url.Action("CreateAgreedValueContract", "Contract", new { ContextID = ContextAssetID }) });

                    case "rate":
                        if (ContractOptions.Get<bool>(ContractOptions.RateBasedContractsEnabled))
                        {
                            return ExtendedJson(new { success = true, url = Url.Action("CreateRateValueContract", "Contract", new { ContextID = ContextAssetID }) });
                        }
                        else
                        {
                            return ExtendedJson(new { success = true, url = Url.Action("CreateAgreedValueContract", "Contract", new { ContextID = ContextAssetID }) });
                        }
                    default:
                        return ExtendedJson(new
                        {
                            success = false,
                            message = "Unknown contract type '" + model.ContractClassification + "'"
                        });
                }
            }

            ViewBag.AssetID = ContextAssetID;
            return PartialView("Dialog/CreateContract", model);
        }

        /// <summary>
        /// The CreateContractDialog.
        /// </summary>
        /// <param name="receivable">The receivable<see cref="bool"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult CreateContractDialog(bool receivable = false)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return Unauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return Unauthorized();
            }

            ViewBag.AssetID = ContextAssetID;
            return PartialView("Dialog/CreateContract", new CreateContractModel { IsReceivable = receivable });
        }

        /// <summary>
        /// The CreateGuarantee.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult CreateGuarantee()
        {
            GuaranteeEditModel model = new GuaranteeEditModel { GuaranteeID = -1 };
            ViewBag.GuaranteeTypes = contractService.GetGuaranteeTypes();
            return PartialView("EditorTemplates/GuaranteeEditModel", model);
        }

        /// <summary>
        /// The CreateGuarantor.
        /// </summary>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult CreateGuarantor()
        {
            return PartialView("EditorTemplates/GuarantorEditModel", new GuarantorEditModel { GuaranteeGuarantorID = -1 });
        }

        /// <summary>
        /// The CreateOption.
        /// </summary>
        /// <param name="terms">The terms<see cref="List{TermEditModel}"/>.</param>
        /// <param name="count">The count<see cref="int"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult CreateOption(List<TermEditModel> terms, int count = 0)
        {
            ModelState.Clear();
            TermEditModel vm = new TermEditModel { IsOption = true };
            TermEditModel last = terms[0];
            vm.TermStart = last.TermEnd.HasValue ? last.TermEnd.Value.AddDays(1) : last.TermStart.AddDays(1);

            if (last.TermEnd.HasValue)
            {
                vm.TermEnd = vm.TermStart;
                int monthdiff = last.TermEnd.Value.Month - last.TermStart.Month;
                int yeardiff = last.TermEnd.Value.Year - last.TermStart.Year;
                DateTime tempDate = vm.TermEnd.Value.AddMonths(monthdiff).AddYears(yeardiff);
                if (DateTime.DaysInMonth(last.TermEnd.Value.Year, last.TermEnd.Value.Month) == last.TermEnd.Value.Day)
                {
                    vm.TermEnd = new DateTime(tempDate.Year, tempDate.Month, DateTime.DaysInMonth(tempDate.Year, tempDate.Month));
                }
                else if (last.TermEnd.Value.Day > DateTime.DaysInMonth(tempDate.Year, tempDate.Month))
                {
                    vm.TermEnd = new DateTime(tempDate.Year, tempDate.Month, DateTime.DaysInMonth(tempDate.Year, tempDate.Month));
                }
                else
                {
                    vm.TermEnd = new DateTime(tempDate.Year, tempDate.Month, last.TermEnd.Value.Day);
                }
            }
            vm.TermName = "Option " + count;
            if (!String.IsNullOrEmpty(Request["VaryLease"]))
            {
                ViewBag.VaryLease = true;
                ViewBag.ContractIsLockedDown = false;
            }
            return ExtendedJson(new { success = true, row = RenderVariantPartialViewToString("EditorTemplates/TermEditModel", vm) }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The CreateRateValueContract.
        /// </summary>
        /// <param name="createModel">The createModel<see cref="CreateContractModel"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult CreateRateValueContract(CreateContractModel createModel)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return PartialUnauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return PartialUnauthorized();
            }

            VMRateContractEditModel model = new VMRateContractEditModel
            {
                IsReceivable = createModel.IsReceivable
            };
            ContractTypeEditModel ct = contractTypeService.GetContractType(createModel.ContractTypeID);
            model.ContractTypeID = ct.ContractTypeID;
            model.ContractCategory = ct.Category;
            model.ContractType = ct.Name;
            model.Terms = new List<TermEditModel> {
                new VMTermEditModel {
                    TermStart = createModel.ContractStart ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    TermEnd = createModel.ContractEnd ?? (createModel.ContractStart ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)).AddYears(1).AddDays(-1),
                    State = "Exercised",
                    IsOption = false,
                    TermName = "Initial Term"
                }
            };
            model.CustomFieldValues = ct.CustomFields.Where(cf => cf.EntitySubType == (model.IsReceivable ? "Receivable" : "Payable"))
                .SelectMany(c => c.Mappings.Select(m =>
                    m.DefaultValue ?? MappingContext.Instance.Map<CustomFieldEditModel, CustomFieldValueEditModel>(m.CustomField)
                )).ToList();
            int id = 0;
            model.OtherClauses.AddRange(ct.PredefinedClauses.Where(c => !model.OtherClauses.Any(c2 => c2.Category == c.Category && c2.Clause == c.Clause)).Select(c => new ContractClauseEditModel
            {
                Clause = c.Clause,
                Category = c.Category,
                ClauseText = "",
                IsActive = c.IsRequired,
                IsPredefinedClause = true,
                ContractClauseID = --id,
                ContractID = model.ContractID
            }));
            SetupEditViewBag(model);
            return PartialView("EditorTemplates/ContractEditModel", model);
        }

        /// <summary>
        /// Create a new rate based review.
        /// </summary>
        /// <param name="terms">  list of terms currently on the contract.</param>
        /// <param name="reviews">list of reviews currently on the contract.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult CreateRBReview(List<TermEditModel> terms, List<VMRateReviewEditModel> reviews)
        {
            ModelState.Clear();
            reviews = (reviews ?? new List<VMRateReviewEditModel>()).OrderBy(r => r.ReviewDate).ToList();
            terms = (terms ?? new List<TermEditModel>()).OrderBy(t => t.TermStart).ToList();

            if (terms.Count < 1)
            {
                return ExtendedJson(new { success = false, message = "An initial term must be added to the contract before costs and reviews can be defined" });
            }

            DateTime reviewDate = DateTime.Now;
            if (reviews.Count == 1)
            {
                reviewDate = reviews[0].ReviewDate.AddMonths(1);
            }
            else if (reviews.Count > 1)
            {
                reviewDate = reviews[reviews.Count - 1].ReviewDate.AddDays((reviews[reviews.Count - 1].ReviewDate - reviews[reviews.Count - 2].ReviewDate).TotalDays);
            }

            return ExtendedJson(new
            {
                success = true,
                html = RenderVariantPartialViewToString("EditorTemplates/RateReviewEditModel", new VMRateReviewEditModel
                {
                    Guid = Guid.NewGuid().ToString(),
                    ReviewDate = reviewDate,
                    IsNew = true
                })
            });
        }

        /// <summary>
        /// The CreateSubContract.
        /// </summary>
        /// <param name="createModel">The createModel<see cref="CreateSubContractModel"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult CreateSubContract(CreateSubContractModel createModel)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return PartialUnauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return PartialUnauthorized();
            }

            if (createModel.ParentContracts.Count > 1)
            {
                ModelState.AddModelError("ParentContracts", "Multiple Parent contracts is currently not supported");
            }
            //Validation failed
            if (createModel.ParentContracts.Count == 0 || createModel.ParentContracts[0].SubContractMappings.Count == 0)
            {
                ViewBag.ContextID = ContextAssetID;
                return PartialView("Partial/SubContracts/CreateSubContract", createModel);
            }
            int parentID = createModel.ParentContracts[0].SubContractMappings[0].ParentContractID.Value;
            AgreedValueContractViewModel ParentContract = contractService.GetContractView(parentID) as AgreedValueContractViewModel;
            IEnumerable<AssetViewModel> allAssets = assetService.FindMatchingAssets("", null, status: assetService.GetAssetStatuses().ToArray()).Select(a => SimpleMapper.Map<AssetListModel, AssetViewModel>(a));
            Dictionary<int, AssetViewModel> Assets = allAssets
                .Where(a => ParentContract.Assets().Contains(a.AssetID)).ToDictionary(a => a.AssetID, a => a);
            Dictionary<int, IGrouping<int, AssetViewModel>> ChildAssets = allAssets.Where(a => a.ParentID.HasValue && ParentContract.Assets().Contains(a.ParentID.Value))
                .GroupBy(a => a.ParentID.Value, a => a)
                .ToDictionary(a => a.Key, a => a);
            createModel.ParentContracts.ForEach(sm =>
            {
                sm.ParentContract = ParentContract;
                sm.ParentContractID = ParentContract.ContractID;
                sm.SubContractMappings.ForEach(sm2 =>
                {
                    sm2.ExistingChildAssets = ChildAssets.ContainsKey(sm2.ParentAssetID) ? ChildAssets[sm2.ParentAssetID].ToList() : new List<AssetViewModel>();
                    sm2.ParentContract = ParentContract;
                    sm2.ParentAsset = Assets[sm2.ParentAssetID];
                    sm2.Asset = sm2.SubContractOptions == VMSubContractMappingModel.SubContractAssetOptions.CreateNewAsset ? null : allAssets.FirstOrDefault(a => sm2.AssetID == a.AssetID);
                    sm2.AssetID = sm2.SubContractOptions == VMSubContractMappingModel.SubContractAssetOptions.CreateNewAsset ? sm2.ChildAssetDetails.ID :
                        sm2.SubContractOptions == VMSubContractMappingModel.SubContractAssetOptions.UseParent ? sm2.ParentAssetID : sm2.AssetID;
                });
            });

            Dictionary<int, decimal> totals = GetParentContractROUTotals(contractService.GetContractEdit(parentID) as AgreedValueContractEditModel, null);
            foreach (VMSubContractMappingModel mapping in createModel.ParentContracts[0].SubContractMappings)
            {
                totals[mapping.ParentAssetID] += mapping.Percentage;
            }

            totals.Where(r => r.Value > 100).ToList()
                .ForEach(kvp =>
                {
                    VMSubContractMappingModel sc = createModel.ParentContracts[0].SubContractMappings
                        .First(r => r.ParentAssetID == kvp.Key);

                    ModelState.AddModelError("ParentContracts.SubContractMappings.Percentage",
                        string.Format("Total ROU for {0} exceeds 100% - current total is {1}",
                        sc.ParentAsset.FullName, kvp.Value));
                });
            TryValidateModel(createModel);
            ModelState.RemoveAllForKeyPrefix("ParentContracts[0].ParentContract");
            ModelState.RemoveAllForKeyPrefix("ParentContracts[0].SubContractMappings[0].ParentContract");
            if (ModelState.Any(a => a.Value.Errors.Count > 0))
            {
                ViewBag.ContextID = ContextAssetID;
                return PartialView("Partial/SubContracts/CreateSubContract", createModel);
            }
            AssetViewModel asset = assetService.GetAssetView(ContextAssetID, false, false, false, false, false);
            VMSubContractEditModel model = new VMSubContractEditModel
            {
                Description = createModel.Description,
                IsReceivable = createModel.IsReceivable,
                ParentContracts = createModel.ParentContracts
            };
            ContractTypeEditModel ct = contractTypeService.GetContractType(createModel.ContractTypeID);
            model.ContractTypeID = ct.ContractTypeID;
            model.ContractCategory = ct.Category;
            model.Terms = new List<TermEditModel> {
                new VMTermEditModel {
                    TermStart = createModel.ContractStart ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    TermEnd = createModel.ContractEnd ?? (createModel.ContractStart ??
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)).AddYears(1).AddDays(-1),
                    State = "Exercised",
                    IsOption = false,
                    TermName = "Initial Term"
                }
            };
            if (asset.Address != null)
            {
                model.CurrencyID = localeService.GetCountryByID(asset.Address.CountryID).DefaultCurrency.CurrencyID;
            }
            model.MakeGoodDateOfObligation = model.Terms[0].TermEnd.Value;
            //
            model.CustomFieldValues = ct.CustomFields.Where(cf => cf.EntitySubType == (model.IsReceivable ? "Receivable" : "Payable"))
                .SelectMany(c => c.Mappings.Select(m =>
                    m.DefaultValue ?? MappingContext.Instance.Map<CustomFieldEditModel, CustomFieldValueEditModel>(m.CustomField)
                )).ToList();
            SetupEditViewBag(model);
            return PartialView("EditorTemplates/ContractEditmodel", model);
        }

        /// <summary>
        /// The DeleteContract.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="deleteAll">The deleteAll<see cref="bool"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult DeleteContract(int ID, bool deleteAll = false)
        {
            if (!(UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit) && UserContext.Current.HasPermission(AssetManagementContractsPermissions.Delete)))
            {
                return JsonUnauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return JsonUnauthorized();
            }

            try
            {
                contractService.DeleteContract(ID, deleteAll);
                return ExtendedJson(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return ExtendedJson(new { success = false, e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The DeleteLeaseAccountingReview.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult DeleteLeaseAccountingReview(int ID)
        {
            if (!UserContext.Current.EvaluateAccess(true, TestAssetIsAccessible, LeaseAccountingReviewPermissions.Landing, LeaseAccountingReviewPermissions.Delete))
            {
                return JsonUnauthorized();
            }

            LeaseAccountingReviewEditModel review = leaseAccountingService.GetLeaseAccountingReviewEdit(ID);
            if (review == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review does not exist"
                });
            }

            if (review.ApprovedDateTime.HasValue)
            {
                return ExtendedJson(new
                {
                    sucess = false,
                    message = "The " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review has been appproved. This review cannot be deleted"
                });
            }

            try
            {
                leaseAccountingService.DeleteLeaseAccountingReview(ID);

                return ExtendedJson(new
                {
                    success = true,
                    message = "The " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review has been successfully marked as deleted",
                    row = RenderVariantPartialViewToString("Tabs/WizardPages/Partial/LeaseAccountingReviewRow", leaseAccountingService.GetLeaseAccountingReviewEdit(ID))
                });
            }
            catch (Exception ex)
            {
                return ExtendedJson(new
                {
                    sucess = false,
                    message = ex.Message

                });
            }

        }

        /// <summary>
        /// The DeleteSubContractMapping.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [HttpPost]
        public JsonResult DeleteSubContractMapping(int ID)
        {
            try
            {
                contractService.DeleteSubContractMapping(ID);
                return Json(new { success = true });
            }
            catch (DomainValidationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// The DownloadDocument.
        /// </summary>
        /// <param name="filekey">The filekey<see cref="string"/>.</param>
        /// <returns>The <see cref="FileResult"/>.</returns>
        public FileResult DownloadDocument(string filekey)
        {
            string filename = System.IO.File.ReadAllText(Path.GetTempPath() + filekey + ".def");
            return File(System.IO.File.ReadAllBytes(Path.GetTempPath() + filekey + ".docx"), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", filename.EndsWith(".docx") ? filename : filename + ".docx");
        }

        /// <summary>
        /// The DownloadPDFDocument.
        /// </summary>
        /// <param name="filekey">The filekey<see cref="string"/>.</param>
        /// <returns>The <see cref="FileResult"/>.</returns>
        public FileResult DownloadPdfDocument(string filekey)
        {
            if (XSettings.InstallLicense("X/VKS0cNn5FhpydaGfTQKt+0efQWCtVwkfTQwuG8Xh9klgnCfSW7KpFWQ0lkwg8KCtU34j9HuSERr6IiQbd4xFMhfGGVB3M/3TFMO/XgBjbi1y7S5MlUFrjUWBKMcmImUL1oUMFb8wtwCFVMoSiSIEERXiebQ2W5r8l4z1spFM/G3rsp8hHg4WTXrL0o4wVRZkwX2VEW83TPKiUtWZBusSRG+WPNBtZycrM="))
            {
                //Convert Word file to HTML
                byte[] byteArray = System.IO.File.ReadAllBytes(Path.GetTempPath() + filekey + ".docx");

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(byteArray, 0, byteArray.Length);
                    WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true);

                    XElement html = HtmlConverter.ConvertToHtml(doc, new HtmlConverterSettings());

                    System.IO.File.WriteAllText(Path.GetTempPath() + filekey + ".html", html.ToStringNewLineOnAttributes());
                    doc.Close();
                }

                //Convert HTML to PDF
                Doc theDoc = new Doc();
                theDoc.MediaBox.String = "A4";
                theDoc.Rect.String = "A4";
                theDoc.Rect.Width = 8.27 * 72;//getPointSize(8.27, resolution); // 8.27 inches wide
                theDoc.Rect.Height = 11.69 * 72; // 11.69 inches long
                theDoc.MediaBox.String = "0 0 " + theDoc.Rect.Width + " " + theDoc.Rect.Height;


                string htmlFile = System.IO.File.ReadAllText(Path.GetTempPath() + filekey + ".html");
                var theID = theDoc.AddImageHtml(htmlFile);
                //now accommodate mult pages
                while (theDoc.Chainable(theID))
                {
                    theDoc.Page = theDoc.AddPage();
                    theID = theDoc.AddImageToChain(theID);
                }

                // Flatten each page
                for (int i = 1; i <= theDoc.PageCount; i++)
                {
                    theDoc.PageNumber = i;

                    theDoc.FrameRect();
                    // Add a page count to each page
                    //theDoc.Flatten();
                }
                theDoc.FrameRect();

                theDoc.Save(Path.GetTempPath() + filekey + ".pdf");

                string filenamePdf = System.IO.File.ReadAllText(Path.GetTempPath() + filekey + ".def").Replace(".docx", ".pdf");
                return File(System.IO.File.ReadAllBytes(Path.GetTempPath() + filekey + ".pdf"), "application/pdf", filenamePdf.EndsWith(".pdf") ? filenamePdf : filenamePdf + ".pdf");
            }
            else throw new Exception("Could not install licnese");
        }

        /// <summary>
        /// The EditAssetScheduleItem.
        /// </summary>
        /// <param name="assetID">.</param>
        /// <param name="IsPrimaryAsset">.</param>
        /// <param name="assetScheduleID">.</param>
        /// <param name="AssetSchedule">This is a hack. gotten as a list because it does a serialize of the table row.</param>
        /// <returns>.</returns>
        public ActionResult EditAssetScheduleItem(int assetID, bool IsPrimaryAsset, int assetScheduleID = 0, List<ContractAssetScheduleItemEditModel> AssetSchedule = null)
        {
            AssetEditModel assetModel = assetService.GetAssetEdit(assetID);
            ContractAssetScheduleItemEditModel model = AssetSchedule?.FirstOrDefault();
            ViewBag.IsVaryLease = IsVaryLease;

            if (model == null)
            {
                model = new ContractAssetScheduleItemEditModel
                {
                    ID = assetScheduleID,
                    Asset = assetModel.Name,
                    BusinessUnit = assetModel.BusinessUnit,
                    BusinessUnitID = assetModel.BusinessUnitID,
                    LegalEntity = assetModel.LegalEntity,
                    LegalEntityID = assetModel.LegalEntityID
                };
            }
            if (IsVaryLease && assetScheduleID > 0)
                return PartialView("DisplayTemplates/ContractAssetScheduleItemEditModel", model);
            return PartialView("EditorTemplates/ContractAssetScheduleItemEditModel", model);
        }

        /// <summary>
        /// The EditAssetScheduleItem.
        /// </summary>
        /// <param name="assetID">.</param>
        /// <param name="IsPrimaryAsset">.</param>
        /// <param name="assetScheduleID">.</param>
        /// <param name="AssetSchedule">This is a hack. gotten as a list because it does a serialize of the table row.</param>
        /// <returns>.</returns>
        public ActionResult EditLAAssetScheduleItem(int assetID, int contractID, int assetScheduleID = 0)
        {
            AssetEditModel assetModel = assetService.GetAssetEdit(assetID);

            AgreedValueContractEditModel contract = contractService.GetContractEdit(contractID) as AgreedValueContractEditModel;
            ContractAssetScheduleItemEditModel assetSchedule = contract.AssetSchedule.FirstOrDefault(a => a.AssetID == assetID);
            ViewBag.Statuses = assetService.GetAssetStatuses();
            ViewBag.Countries = localeService.GetAllCountries();
            ContractAddressAssetScheduleItemEditModel model = new ContractAddressAssetScheduleItemEditModel();
            model.ContractAssetScheduleItem.AssetID = assetID;
            model.ContractAssetScheduleItem.ContractID = contractID;
            model.ContractAssetScheduleItem.Asset = assetModel.Name;
            model.ContractAssetScheduleItem.AvailableForUseDate = assetSchedule.AvailableForUseDate;
            model.ContractAssetScheduleItem.DepreciationStartDate = assetSchedule.DepreciationStartDate;
            model.ContractAssetScheduleItem.CostCenter = assetSchedule.CostCenter;
            model.ContractAssetScheduleItem.GLCode = assetSchedule.GLCode;
            model.ContractAssetScheduleItem.UnitPrice = assetSchedule.UnitPrice;
            model.ContractAssetScheduleItem.AssetOwner = assetSchedule.AssetOwner;
            model.ContractAssetScheduleItem.AssetOwnerID = assetSchedule.AssetOwnerID;
            model.ContractAssetScheduleItem.AssetUser = assetSchedule.AssetUser;
            model.ContractAssetScheduleItem.AssetUserID = assetSchedule.AssetUserID;
            model.ContractAssetScheduleItem.BusinessUnit = assetModel.BusinessUnit;
            model.ContractAssetScheduleItem.BusinessUnitID = assetModel.BusinessUnitID;
            model.ContractAssetScheduleItem.LegalEntity = assetModel.LegalEntity;
            model.ContractAssetScheduleItem.LegalEntityID = assetModel.LegalEntityID;
            model.ContractAssetScheduleItem.ValidFrom = assetSchedule.ValidFrom;
            model.ContractAssetScheduleItem.ValidTo = assetSchedule.ValidTo;
            model.Address.AddressID = assetModel.Address.AddressID;
            model.Address.City = assetModel.Address.City;
            model.Address.CountryID = assetModel.Address.CountryID;
            model.Address.CountryName = assetModel.Address.CountryName;
            model.Address.IsDefaultMailingAddress = assetModel.Address.IsDefaultMailingAddress;
            model.Address.LA_ID = assetModel.Address.LA_ID;
            model.Address.Line1 = assetModel.Address.Line1;
            model.Address.Line2 = assetModel.Address.Line2;
            model.Address.Longitude = assetModel.Address.Longitude;
            model.Address.Latitude = assetModel.Address.Latitude;
            model.Address.PostCode = assetModel.Address.PostCode;
            model.Address.StateAbbreviation = assetModel.Address.StateAbbreviation;
            model.Address.StateID = assetModel.Address.StateID;


            return PartialView("EditorTemplates/ContractLAAssetScheduleItemEditModel", model);
        }

        /// <summary>
        /// Edit an existing Fixed, Market, or CPI region.
        /// </summary>
        /// <param name="reviews">   list of all reviews currently on the contract</param>
        /// <param name="guid">      guid of the review to be edited</param>
        /// <param name="terms">     list of all the terms currently on the contract</param>
        /// <param name="currencyID">currently selected currency id</param>
        /// <param name="scope"></param>
        /// <param name="ParentContracts">The ParentContracts<see cref="List{VMParentContractsModel}"/></param>
        /// <param name="AssetSchedule">The AssetSchedule<see cref="List{ContractAssetScheduleItemEditModel}"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        [HttpPost]
        public ExtendedJsonResult EditAVReview(List<VMAgreedValueReviewEditModel> reviews, string guid, List<TermEditModel> terms, int currencyID, string scope = "", List<VMParentContractsModel> ParentContracts = null, List<ContractAssetScheduleItemEditModel> AssetSchedule = null)
        {
            ModelState.Clear();
            ViewBag.VaryLease = Request["VaryLease"] == "True";
            reviews = (reviews ?? new List<VMAgreedValueReviewEditModel>()).OrderBy(r => r.ReviewDate).ToList();
            terms = (terms ?? new List<TermEditModel>()).OrderBy(t => t.TermStart).ToList();

            if (terms.Count < 1)
            {
                return ExtendedJson(new { success = false, message = "An initial term must be added to the contract before costs and reviews can be defined" });
            }

            VMAgreedValueReviewEditModel review = reviews.SingleOrDefault(r => r.Guid == guid);
            if (review == null)
            {
                return ExtendedJson(new { success = false, message = "The review does not exist and cannot be edited. Please try again" });
            }

            if (scope == "limited")
            {
                return LimitedEditAVReview(review, currencyID);
            }

            ViewBag.CurrencyFormat = localeService.GetCurrency(currencyID).FormatString;
            review.IsNew = false;
            IEnumerable<CostCategoryListModel> categories = costCategoryService.GetAllCostCategories();
            Dictionary<string, string> cpiregions = contractService.GetCPIRegionList().ToDictionary(r => r.ID.ToString(), r => r.Name);
            List<SelectItem> assetlist = assetService.GetAssetSelectList(currencyID);
            if (ParentContracts != null)
            {
                ParentContracts.SelectMany(pc => pc.SubContractMappings.Select(c => c.ChildAssetDetails)).ToList().ForEach(a =>
                {
                    assetlist.Add(new SelectItem { Key = a.ID.ToString(), Name = a.Name, Visible = true });
                });
            }
            if (AssetSchedule != null)
            {
                AssetSchedule.ForEach(a =>
                {
                    assetlist.Add(new SelectItem { Key = a.AssetID.ToString(), Name = a.Asset, Visible = true });
                });
            }
            return ExtendedJson(new
            {
                success = true,
                type = review.ReviewType,
                html = RenderVariantPartialViewToString("EditorTemplates/AgreedValueReviewEditModel", review),
                rows = review.Costs,
                cpiregions,
                categories = categories.ToDictionary(c => c.CostCategoryID.ToString(), c => c.DisplayName()),
                assets = assetlist,
                jurisdictions = localeService.GetTaxJurisdictions().Values.ToDictionary(j => j.Code, j => new
                {
                    code = j.Code,
                    name = j.Name,
                    taxrates = (IList<VMTaxRateViewModel>)null
                })
            });
        }

        /// <summary>
        /// The EditBreakClause.
        /// </summary>
        /// <param name="breakclauses">The breakclauses<see cref="List{BreakClauseEditModel}"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult EditBreakClause(List<BreakClauseEditModel> breakclauses)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return JsonUnauthorized();
            }

            if (breakclauses.Count < 1)
            {
                return ExtendedJson(new { success = false });
            }

            return ExtendedJson(new { success = true, html = RenderVariantPartialViewToString("EditorTemplates/BreakClauseEditModel", breakclauses[0]) });
        }

        /// <summary>
        /// The EditClause.
        /// </summary>
        /// <param name="contractTypeId">The contractTypeId<see cref="int"/>.</param>
        /// <param name="otherClauses">The clause to be edited in list format, see <see cref="List{ContractClauseEditModel}"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult EditClause(int contractTypeId, List<ContractClauseEditModel> otherClauses)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return JsonUnauthorized();
            }

            // should not be calling edit clause if there are no clauses
            if (otherClauses == null || otherClauses.Count < 1)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(new InvalidOperationException("Attempted to edit a clause but no clause data passed in"));
                return ExtendedJson(new { success = false, message = "An unexpected error occurred, please try again." });
            }
            ContractClauseEditModel model = otherClauses[0];
            if (!model.IsActive)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "Cannot edit a clause that has been excluded from the contract"
                });
            }

            ContractTypeEditModel contractType = contractTypeService.GetContractType(contractTypeId);
            ContractTypeClauseEditModel predefinedClause = contractType.PredefinedClauses.FirstOrDefault(c => c.Category == model.Category && c.Clause == model.Clause);
            if (predefinedClause != null)
            {
                model.IsPredefinedClause = true;
                model.IsRequired = predefinedClause.IsRequired;
                model.YearFieldMode = predefinedClause.PredefinedClause.YearFieldMode;
                model.PercentageFieldMode = predefinedClause.PredefinedClause.PercentageFieldMode;
                model.AreaFieldMode = predefinedClause.PredefinedClause.AreaFieldMode;
                model.AmountPayableMode = predefinedClause.PredefinedClause.AmountPayableMode;
                model.PayableToMode = predefinedClause.PredefinedClause.PayableToMode;
                model.AmountReceivableMode = predefinedClause.PredefinedClause.AmountReceivableMode;
                model.ReceivableFromMode = predefinedClause.PredefinedClause.ReceivableFromMode;
            }
            ModelState.Clear();
            ViewBag.ClauseCategories = GenerateClauseHeirarchy(otherClauses[0]);
            return ExtendedJson(new { success = true, html = RenderVariantPartialViewToString("EditorTemplates/ContractClauseEditModel", otherClauses[0]) });
        }

        /// <summary>
        /// The MapAgreedValueContractToVM.
        /// </summary>
        /// <param name="editModel">The editModel<see cref="AgreedValueContractEditModel"/>.</param>
        /// <returns>The <see cref="VMAgreedValueContractEditModel"/>.</returns>
        private VMAgreedValueContractEditModel MapAgreedValueContractToVM(AgreedValueContractEditModel editModel)
        {
            VMAgreedValueContractEditModel avcontract = editModel.ParentContracts.Count > 0 ? SimpleMapper.MapNew<AgreedValueContractEditModel, VMSubContractEditModel>(editModel) :
                SimpleMapper.MapNew<AgreedValueContractEditModel, VMAgreedValueContractEditModel>(editModel);
            if (avcontract is VMSubContractEditModel)
            {
                (avcontract as VMSubContractEditModel).ParentContracts =
                    editModel.ParentContracts.GroupBy(pc => pc.ParentContractID).Select(pc => new VMParentContractsModel
                    {
                        SubContractMappings = pc.Select(c => SimpleMapper.Map<SubContractMappingEditModel, VMSubContractMappingModel>(c)).ToList()
                    }).ToList();
            }
            avcontract.VendorName = editModel.Vendor;
            avcontract.Reviews = MapAgreedValueReviewToVM(editModel, avcontract.Templates);
            avcontract.Reviews.Sort((r1, r2) => (r1.ActionedReview != null ? r1.ActionedReview.EffectiveDate : r1.ReviewDate).CompareTo(r2.ActionedReview != null ? r2.ActionedReview.EffectiveDate : r2.ReviewDate));
            avcontract.Terms.Sort((t1, t2) => t1.TermStart.CompareTo(t2.TermStart));
            avcontract.BreakClauses.Sort((b1, b2) => b1.ExerciseStart.CompareTo(b2.ExerciseStart));
            // remove from contract templates where template is part of an actioned review
            avcontract.Templates.Where(t => t.InvoiceTemplateID > 0
                && avcontract.Reviews
                    .Where(r => r.ActionedReview != null)
                    .Any(r => r.ActionedReview.Templates.Union(r.ActionedReview.UnchangedTemplates)
                        .Any(t2 => t2.InvoiceTemplateID == t.InvoiceTemplateID))).ToList().ForEach(t => avcontract.Templates.Remove(t));
            avcontract.Templates.ForEach(t => { t.Modified = false; t.New = false; });
            avcontract.Templates.Sort((t1, t2) => t1.StartDate.CompareTo(t2.StartDate));
            avcontract.Invoices = invoiceService.GetInvoiceListForContract(avcontract.ContractID);
            return avcontract;
        }

        /// <summary>
        /// The MapAgreedValueReviewToVM.
        /// </summary>
        /// <param name="avcontract">The avcontract<see cref="AgreedValueContractEditModel"/>.</param>
        /// <param name="Templates">The Templates<see cref="List{VMInvoiceTemplateEditModel}"/>.</param>
        /// <returns>The <see cref="List{VMAgreedValueReviewEditModel}"/>.</returns>
        private List<VMAgreedValueReviewEditModel> MapAgreedValueReviewToVM(AgreedValueContractEditModel avcontract, List<VMInvoiceTemplateEditModel> Templates)
        {
            AgreedValueActionedReviewEditModel previousActionedReview = null;
            List<InvoiceTemplateEditModel> reviewTemplateList = invoiceService.GetInvoiceTemplatesByContractReviews(avcontract.ContractID);
            Dictionary<int, InvoiceTemplateEditModel> reviewTemplateDictionary = reviewTemplateList.ToDictionary(c => c.InvoiceTemplateID);
            Dictionary<int, InvoiceTemplateEditModel> templateCostsDictionary = reviewTemplateList.SelectMany(c => c.Costs).ToDictionary(c => c.TemplateCostId, c => reviewTemplateDictionary[c.InvoiceTemplateID]);
            Dictionary<int, string> cpiregions = localeService.GetAllCPIRegions().ToDictionary(c => c.RegionID, c => c.Name);
            List<VMAgreedValueReviewEditModel> reviewVMs = avcontract.Reviews.OrderBy(r => r.ReviewDate).Select(r =>
            {
                VMAgreedValueReviewEditModel r2 = SimpleMapper.MapNew<AgreedValueReviewEditModel, VMAgreedValueReviewEditModel>(r);
                if (r2.ReviewType == "Commencing")
                {
                    r2.Guid = "costs";
                }

                if (r.ActionedReview == null)
                {
                    return r2;
                }
                // Actioned Costs:
                // 1. Costs that are assigned to a template and that are being actioned
                // 2. Costs that are not assigned to a template and that are being actioned
                // Unactioned Costs:
                // 1. Costs that are assigned to a template and that are not being actioned,
                //    and no costs on the template are being actioned
                // 2. Costs that are assigned to a template and that are not being actioned,
                //    but share the template with a cost that is
                // 3. Costs that are not assigned to a template and that are not being actioned
                List<AgreedValueContractCostEditModel> actionedCosts = r.ActionedReview.Costs
                    .Where(c => r.ReviewType == "Commencing" || (r.ReviewType == "Adjustment" && r.Costs.Count < 1) || r.Costs.Any(rc => rc.AssetID == c.AssetID && rc.CategoryID == c.CategoryID && (r.ReviewType == "Market" || r.ReviewType == "CPI" || rc.Label == c.Label)))
                    .ToList();

                List<AgreedValueContractCostEditModel> unactionedCosts = r.ActionedReview.Costs
                    .Where(c => !actionedCosts.Contains(c))
                    .ToList();

                List<AgreedValueContractCostEditModel> actionedCostsOnTemplates = actionedCosts
                    .Where(c => c.TemplateCostID != null && c.TemplateCostID.Value > 0)
                    .ToList();

                List<AgreedValueContractCostEditModel> actionedCostsNotOnTemplates = actionedCosts
                    .Where(c => c.TemplateCostID == null || c.TemplateCostID.Value < 1)
                    .ToList();

                List<AgreedValueContractCostEditModel> unactionedCostsOnTemplates = unactionedCosts
                    .Where(c => c.TemplateCostID != null && c.TemplateCostID.Value > 0)
                    .ToList();

                List<AgreedValueContractCostEditModel> unactionedCostsNotOnTemplates = unactionedCosts
                    .Where(c => c.TemplateCostID == null || c.TemplateCostID.Value < 1)
                    .ToList();

                List<AgreedValueContractCostEditModel> unactionedCostsSharedTemplates = unactionedCostsOnTemplates
                    .Where(c => actionedCostsOnTemplates.Any(c2 => c2.TemplateCostID != null && c.TemplateCostID != null && templateCostsDictionary.ContainsKey(c2.TemplateCostID.Value) && templateCostsDictionary[c2.TemplateCostID.Value].InvoiceTemplateID == templateCostsDictionary[c.TemplateCostID.Value].InvoiceTemplateID))
                    .ToList();

                List<AgreedValueContractCostEditModel> unactionedCostsUnsharedTemplates = unactionedCostsOnTemplates
                    .Where(c => !actionedCostsOnTemplates.Any(c2 => c2.TemplateCostID != null && c.TemplateCostID != null && templateCostsDictionary.ContainsKey(c2.TemplateCostID.Value) && templateCostsDictionary[c2.TemplateCostID.Value].InvoiceTemplateID == templateCostsDictionary[c.TemplateCostID.Value].InvoiceTemplateID))
                    .ToList();

                // from a view perspective, templates that are shared will be closed

                r2.ActionedReview = new VMActionAVReviewModel
                {
                    ActionedDate = r.ActionedReview.ActionedDate,
                    EffectiveDate = r.ActionedReview.EffectiveDate,
                    Guid = r2.Guid,
                    Priority = r.ActionedReview.Priority,
                    ReviewDate = r2.ReviewDate,
                    ReviewType = r2.ReviewType,
                    RemeasurementDate = r2.RemeasurementDate,
                    Notes = r2.Notes,
                    ReviewID = r2.ReviewID,
                    IsNew = false,
                    ActionedCosts_NotInvoiced = actionedCostsNotOnTemplates
                        .Select(c =>
                        {
                            VMAgreedValueContractCostEditModel cost = SimpleMapper.MapNew<AgreedValueContractCostEditModel, VMAgreedValueContractCostEditModel>(c);
                            if (r.ReviewType == "CPI" || r.ReviewType == "Market")
                            {
                                AgreedValueReviewCostEditModel reviewCost = r.Costs.First(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID);
                                if (reviewCost?.CPIRegionID.HasValue == true && reviewCost.CPIRegionID.Value > 0)
                                {
                                    cost.CPIRegion = cpiregions[reviewCost.CPIRegionID.Value];
                                }
                                else
                                {
                                    cost.CPIRegion = "";
                                }

                                cost.Cap = reviewCost == null ? 0 : reviewCost.Cap ?? 0;
                                cost.Collar = reviewCost == null ? 0 : reviewCost.Collar ?? 0;
                                cost.Estimate = reviewCost == null ? 0 : reviewCost.Estimate ?? 0;
                                cost.Plus = reviewCost == null ? 0 : reviewCost.Plus ?? 0;
                            }
                            cost.Actioned = true;
                            return cost;
                        }).ToList(),
                    UnactionedCosts_NotInvoiced = new List<VMAgreedValueContractCostEditModel>(),
                    Templates = actionedCostsOnTemplates.Union(unactionedCostsSharedTemplates)
                    .Where(c => c.TemplateCostID != null && templateCostsDictionary.ContainsKey(c.TemplateCostID.Value))
                    .GroupBy(c => templateCostsDictionary[c.TemplateCostID.Value].InvoiceTemplateID).Select(g =>
                    {
                        InvoiceTemplateEditModel theTemplate = reviewTemplateDictionary[g.Key];
                        VMActionAVReviewModel.VMActionAVReviewTemplateModel template = new VMActionAVReviewModel.VMActionAVReviewTemplateModel
                        {
                            InvoiceTemplateID = g.Key,
                            VendorID = theTemplate.VendorID,
                            VendorName = theTemplate.VendorName,
                            TemplateVendorID = theTemplate.VendorID,
                            TemplateVendorName = theTemplate.VendorName,
                            ActionedCosts = g.Where(c => actionedCostsOnTemplates.Contains(c)).Select(c =>
                            {
                                VMAgreedValueContractCostEditModel c2 = SimpleMapper.MapNew<AgreedValueContractCostEditModel, VMAgreedValueContractCostEditModel>(c);
                                c2.Actioned = true;
                                c2.OriginalTemplateID = g.Key;
                                if (r.ReviewType == "CPI" || r.ReviewType == "Market")
                                {
                                    AgreedValueReviewCostEditModel reviewCost = r.Costs.First(c3 => c3.AssetID == c.AssetID && c2.CategoryID == c.CategoryID);
                                    if (reviewCost?.CPIRegionID.HasValue == true && reviewCost.CPIRegionID.Value > 0)
                                    {
                                        c2.CPIRegion = cpiregions[reviewCost.CPIRegionID.Value];
                                    }
                                    else
                                    {
                                        c2.CPIRegion = "";
                                    }

                                    c2.Cap = reviewCost == null ? 0 : reviewCost.Cap ?? 0;
                                    c2.Collar = reviewCost == null ? 0 : reviewCost.Collar ?? 0;
                                    c2.Estimate = reviewCost == null ? 0 : reviewCost.Estimate ?? 0;
                                    c2.Plus = reviewCost == null ? 0 : reviewCost.Plus ?? 0;
                                }

                                return c2;
                            }).ToList(),
                            UnchangedCosts = g.Where(c => unactionedCostsSharedTemplates.Contains(c)).Select(c =>
                            {
                                VMAgreedValueContractCostEditModel c2 = SimpleMapper.MapNew<AgreedValueContractCostEditModel, VMAgreedValueContractCostEditModel>(c);
                                c2.Actioned = false;
                                c2.OriginalTemplateID = g.Key;
                                c2.CPIRegion = "";
                                return c2;
                            }).ToList(),
                            Description = theTemplate.Description,
                            FirstInvoiceDate = theTemplate.FirstInvoiceDate,
                            Frequency = theTemplate.Frequency,
                            Pattern = theTemplate.Pattern,
                            InvoiceGroup = theTemplate.Group,
                            InvoiceTypeID = theTemplate.InvoiceTypeID
                        };
                        return template;
                    }).ToList(),
                    UnchangedTemplates = unactionedCostsUnsharedTemplates
                    .Where(c => c.TemplateCostID != null && templateCostsDictionary.ContainsKey(c.TemplateCostID.Value))
                    .GroupBy(c => templateCostsDictionary[c.TemplateCostID.Value].InvoiceTemplateID).Select(g =>
                    {
                        InvoiceTemplateEditModel theTemplate = reviewTemplateDictionary[g.Key];
                        VMActionAVReviewModel.VMActionAVReviewTemplateModel template = new VMActionAVReviewModel.VMActionAVReviewTemplateModel
                        {
                            UnchangedCosts = g.Select(c =>
                            {
                                VMAgreedValueContractCostEditModel c2 = SimpleMapper.MapNew<AgreedValueContractCostEditModel, VMAgreedValueContractCostEditModel>(c);
                                c2.Actioned = false;
                                c2.OriginalTemplateID = g.Key;
                                c2.CPIRegion = "";
                                return c2;
                            }).ToList(),
                            InvoiceTemplateID = g.Key,
                            Description = theTemplate.Description,
                            FirstInvoiceDate = theTemplate.FirstInvoiceDate,
                            Frequency = theTemplate.Frequency,
                            Pattern = theTemplate.Pattern,
                            InvoiceGroup = theTemplate.Group,
                            InvoiceTypeID = theTemplate.InvoiceTypeID
                        };
                        return template;
                    }).ToList(),
                    UnchangedCosts = unactionedCostsNotOnTemplates.Select(c => SimpleMapper.MapNew<AgreedValueContractCostEditModel, VMAgreedValueContractCostEditModel>(c)).ToList()
                };
                Templates.Where(t => reviewTemplateList.Any(t2 => t2.InvoiceTemplateID == t.InvoiceTemplateID)).ToList().ForEach(t => t.FromActionedReview = true);
                if (r.ReviewType == "Adjustment")
                {
                    if (previousActionedReview != null)
                    {
                        // populate removed costs
                        List<VMAgreedValueContractCostEditModel> allCosts = r2.ActionedReview.CloneAllCosts();
                        r2.ActionedReview.RemovedCosts = previousActionedReview.Costs.Where(c =>
                                !allCosts.Any(c2 =>
                                    c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID
                                    && c2.Label == c.Label))
                            .Select(c =>
                            {
                                VMAgreedValueContractCostEditModel c3 = SimpleMapper
                                    .MapNew<AgreedValueContractCostEditModel, VMAgreedValueContractCostEditModel>(
                                        c);
                                c3.Actioned = true;
                                return c3;
                            }).ToList();
                    }
                }
                previousActionedReview = r.ActionedReview;

                return r2;
            }).ToList();
            return reviewVMs;
        }

        /// <summary>
        /// The EditContract.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpGet]
        public ActionResult EditContract(int ID)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return Unauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return Unauthorized();
            }

            ContractEditModel vm = contractService.GetContractEdit(ID);
            VMContractEditModel model;
            if (vm == null)
            {
                return RedirectToAction("Index", "Error", new { message = "The contract does not exist and may have been removed by another user" });
            }

            if (vm is AgreedValueContractEditModel editModel)
            {
                model = MapAgreedValueContractToVM(editModel);
                ViewBag.CanMove = editModel.AllAssets().Count == 1;
                //Set up the default AssetSchedule
                if (model.SubjectToLeaseAccounting)
                {
                    if (model.AssetSchedule.Count == 0)
                    {
                        AssetEditModel aEditModel = assetService.GetAssetEdit(ContextAssetID);
                        model.AssetSchedule.Add(new ContractAssetScheduleItemEditModel
                        {
                            ID = -1,
                            Asset = aEditModel.Name,
                            IsPrimaryAsset = true,
                            AssetID = ContextAssetID,
                            BusinessUnit = aEditModel.BusinessUnit,
                            BusinessUnitID = aEditModel.BusinessUnitID,
                            LegalEntity = aEditModel.LegalEntity,
                            LegalEntityID = aEditModel.LegalEntityID
                        });
                    }
                    model.ContractIsLockedDown = !LeaseAccountingOptions.Get<bool>(LeaseAccountingOptions.ContractsAreLockDownEditable) && model.Lifecycle_state != "In-Abstraction";

                    List<LeaseAccountingSyncStatusModel> LeaseAccountingSyncStatus =
                        leaseAccountingService.GetLeaseAccountingReviewSynchronisationStatusByContract(ID);

                    if (LeaseAccountingSyncStatus.Count > 0)
                    {
                        LeaseAccountingSyncStatusModel LastLeaseAccountingSyncStatus =
                       LeaseAccountingSyncStatus.OrderByDescending(r => r.CreatedDate).FirstOrDefault();

                        if (LastLeaseAccountingSyncStatus != null)
                        {
                            bool hasLast_Acct_Approved_or_Rejected = new string[] { "ACCT_APPROVED", "RE_REJECTED", "ACCT_REJECTED" }.Contains(LastLeaseAccountingSyncStatus.LAP_EventCode);
                            model.HasBeenSynchronized = model.SubjectToLeaseAccounting && hasLast_Acct_Approved_or_Rejected;
                        }
                    }
                    else
                    {
                        model.HasBeenSynchronized = true;
                    }
                }
            }
            else
            {
                VMRateContractEditModel rcontract = SimpleMapper.MapNew<RateValueContractEditModel, VMRateContractEditModel>(vm as RateValueContractEditModel);
                rcontract.Reviews.Where(r => r.ActionedReview != null).First(r => r.ReviewDate.Date == rcontract.Terms.OrderBy(t => t.TermStart).First().TermStart.Date).Guid = "costs";
                model = rcontract;
                ViewBag.CanMove = (vm as RateValueContractEditModel).AllAssets().Count == 1;
            }
            ContractTypeEditModel ct = contractTypeService.GetContractType(vm.ContractTypeID);
            model.CustomFieldValues.FillCustomFieldValue();
            model.CustomFieldValues.Where(r => r.EntityID != model.EntityID && r.CustomField.MinimumValues < 1).Select(c => { c.Value = ""; return c; }).ToList();
            model.ContractCategory = ct.Category;

            if (model.IsReadOnly)
            {
                return ViewContract(ID, ContextAssetID);
            }

            ViewBag.SubjectToLeaseAccounting = model.SubjectToLeaseAccounting;
            SetupEditViewBag(model);
            return PartialView("EditorTemplates/ContractEditModel", model);
        }

        /// <summary>
        /// The EditContractDetails.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult EditContractDetails(int ID)
        {
            try
            {
                ContractViewModel contractview = contractService.GetContractView(ID);
                int contextAsset = contractview.Assets().First();
                return Redirect(Url.AssetTabAction("Detail", contextAsset, "contracts", "edit", new { contractid = ID }));
            }
            catch (DomainEntityNotFoundException)
            {
                return RedirectToAction("Partial", "Error", new { message = "The Contract you tried to view could not be found." });
            }
        }

        /// <summary>
        /// The EditGuarantee.
        /// </summary>
        /// <param name="guarantees">The guarantees<see cref="List{GuaranteeEditModel}"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult EditGuarantee(List<GuaranteeEditModel> guarantees)
        {
            ViewBag.GuaranteeTypes = contractService.GetGuaranteeTypes();
            return PartialView("EditorTemplates/GuaranteeEditModel", guarantees[0]);
        }

        /// <summary>
        /// The EditLeasedAssetDetails.
        /// </summary>
        /// <param name="model">The model<see cref="ContractAssetScheduleItemEditModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult EditLeasedAssetDetails(ContractAssetScheduleItemEditModel model)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return Unauthorized();
            }

            if (!assetService.AssetIsEditable(model.AssetID))
            {
                return Unauthorized();
            }
            try
            {
                assetService.GetAssetEdit(model.AssetID);
                return PartialView("EditorTemplates/ContractAssetScheduleItemEditModel", model);
            }
            catch (DomainValidationException ex)
            {
                return RedirectToError(ex.Message);
            }
        }




        /// <summary>
        /// adds a cost adjustment on any date, to move all review costs onto invoices, and retroactively applies it to any actioned invoices.
        /// </summary>
        [HttpPost]
        public ExtendedJsonResult MoveCostsToInvoices(int contractId, string moveToInvoicesAsAtDate)
        {
            try
            {
                if (!DateTime.TryParseExact(moveToInvoicesAsAtDate, UserContext.Current.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var invoiceFrom))
                    throw new ArgumentException(message: "Invalid invoice from start date", nameof(moveToInvoicesAsAtDate));

                contractService.MoveAllCostsToInvoices(contractId, invoiceFrom);
                return ExtendedJson(new { success = true, message = "Review costs have been moved to invoices." });
            }
            catch (Exception ex)
            {
                return ExtendedJson(new { success = false, message = $"Unable to move review costs to invoices: {ex.Message}", ex = ex.Message, trace = ex.StackTrace });
            }

        }


        /// <summary>
        /// The EditNotes.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="notes">The notes<see cref="string"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult EditNotes(int ID, string notes)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.EditNotes)
             && !UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return JsonUnauthorized();
            }

            try
            {
                contractService.EditNotes(ID, notes);
                return ExtendedJson(new { success = true, message = "Contract notes updated" }, JsonRequestBehavior.AllowGet);
            }
            catch (DomainEntityNotFoundException nfex)
            {
                return ExtendedJson(new { success = false, message = nfex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (DomainIntegrityException iex)
            {
                return ExtendedJson(new { success = false, message = iex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (DomainSecurityException sex)
            {
                return ExtendedJson(new { success = false, message = sex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return ExtendedJson(new { success = false, message = "An unexpected error occurred preventing the contract from being updated. Please try again." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Edit an existing RB review (review date and notes basically).
        /// </summary>
        /// <param name="terms">  list of terms currently on the contract.</param>
        /// <param name="reviews">list of reviews currently on the contract.</param>
        /// <param name="guid">   guid of review to edit.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult EditRBReview(List<TermEditModel> terms, List<VMRateReviewEditModel> reviews, string guid)
        {
            ModelState.Clear();
            reviews = (reviews ?? new List<VMRateReviewEditModel>()).OrderBy(r => r.ReviewDate).ToList();
            terms = (terms ?? new List<TermEditModel>()).OrderBy(t => t.TermStart).ToList();

            if (terms.Count < 1)
            {
                return ExtendedJson(new { success = false, message = "An initial term must be added to the contract before costs and reviews can be defined" });
            }

            VMRateReviewEditModel review = reviews.SingleOrDefault(r => r.Guid == guid);
            if (review == null)
            {
                return ExtendedJson(new { success = false, message = "The review does not exist and cannot be edited. Please try again" });
            }

            review.IsNew = false;
            return ExtendedJson(new
            {
                success = true,
                html = RenderVariantPartialViewToString("EditorTemplates/RateReviewEditModel", review)
            });
        }

        /// <summary>
        /// The EditRecurringInvoiceOnAVReview.
        /// </summary>
        /// <param name="terms">The terms<see cref="List{TermEditModel}"/></param>
        /// <param name="currencyID">The currencyID<see cref="int"/></param>
        /// <param name="review">The review<see cref="VMActionAVReviewModel"/></param>
        /// <param name="ParentContracts">The ParentContracts<see cref="List{VMParentContractsModel}"/></param>
        /// <param name="AssetSchedule">The AssetSchedule<see cref="List{ContractAssetScheduleItemEditModel}"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        [HttpPost]
        public ExtendedJsonResult EditRecurringInvoiceOnAVReview(List<TermEditModel> terms, int currencyID, VMActionAVReviewModel review, List<VMParentContractsModel> ParentContracts, List<ContractAssetScheduleItemEditModel> AssetSchedule, int VendorId)
        {
            terms = (terms ?? new List<TermEditModel>()).OrderBy(t => t.TermStart).ToList();
            if (terms.Count < 1)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "Cannot action reviews when there are no defined terms or options on the contract"
                });
            }

            VMActionAVReviewModel.VMActionAVReviewTemplateModel model = new VMActionAVReviewModel.VMActionAVReviewTemplateModel
            {
                FirstInvoiceDate = review.EffectiveDate,
                Frequency = 1,
                Pattern = "Months",
                InvoiceTemplateID = 0,
                TemplateVendorID = VendorId,
                TemplateVendorName = contactService.GetContactDisplayName(VendorId)

            };
            ViewBag.ReviewType = review.ReviewType;
            ViewBag.ContractStart = terms[0].TermStart;
            ViewBag.ContractEnd = terms.Last().TermEnd;
            List<string> groups = invoiceService.GetAllInvoiceGroups().Where(g => !string.IsNullOrWhiteSpace(g)).ToList();
            groups.Add(ClientContext.Current.GetConfigurationSetting("Invoices.DefaultGroup", "Basic Invoice"));
            groups = groups.Distinct().OrderBy(g => g, StringComparer.OrdinalIgnoreCase).ToList();
            if (!string.IsNullOrWhiteSpace(model.InvoiceGroup))
            {
                groups.Add(model.InvoiceGroup);
            }
            groups = groups.Distinct().OrderBy(g => g, StringComparer.OrdinalIgnoreCase).ToList();
            ViewBag.InvoiceGroups = groups.Select(g => new SelectListItem { Text = g, Value = g }).ToList();
            ViewBag.InvoiceTypes = invoiceTypeService.GetInvoiceTypes().Select(t => new SelectListItem { Text = t.Name, Value = t.InvoiceTypeID.ToString() }).ToList();
            Dictionary<string, string> cpiregions = contractService.GetCPIRegionList().ToDictionary(r => r.ID.ToString(), r => r.Name);
            List<SelectItem> assets = assetService.GetAssetSelectList(currencyID);
            if (ParentContracts?.Count > 0)
            {
                assets = assets.Where(a => ParentContracts.Any(sc => sc.SubContractMappings
                    //for a subcontract we want either the asset,parent or context asset so that things don't break
                    .Any(sm => sm.AssetID.ToString() == a.Key || sm.ParentAssetID.ToString() == a.Key
                    || ContextAssetID.ToString() == a.Key))).ToList();
                int tempid = -1;
                assets.AddRange(ParentContracts.SelectMany(pc => pc.SubContractMappings)
                    .Where(sm => sm.SubContractOptions == VMSubContractMappingModel.SubContractAssetOptions.CreateNewAsset)
                    .Select(sm => new SelectItem { Key = tempid--.ToString(), Name = sm.ChildAssetDetails.Name, Visible = true }));
            }
            if (AssetSchedule != null && AssetSchedule.Count > 0)
            {
                assets = assets.Where(a => ContextAssetID.ToString() == a.Key || AssetSchedule.Any(sc => sc.AssetID.ToString() == a.Key)).ToList();
            }
            IEnumerable<CostCategoryListModel> categories = costCategoryService.GetAllCostCategories();

            return ExtendedJson(new
            {
                success = true,
                row = RenderVariantPartialViewToString("Partial/ActionAVReview_InvoiceTemplate", model),
                cpiregions,
                categories = categories.ToDictionary(c => c.CostCategoryID.ToString(), c => c.DisplayName()),
                assets,
                jurisdictions = localeService.GetTaxJurisdictions().Values.ToDictionary(j => j.Code, j => new
                {
                    code = j.Code,
                    name = j.Name,
                    taxrates = (IList<VMTaxRateViewModel>)null
                })
            });
        }

        /// <summary>
        /// The FillTemplate.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="fileid">The fileid<see cref="Guid"/>.</param>
        /// <param name="entryid">The entryid<see cref="int"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult FillTemplate(int id, Guid fileid, int entryid)
        {
            FileEditModel file = fileService.GetFileByID(fileid);
            List<DocumentVariable> variables = documentService.ExtractDocumentVariables(fileid).ToList();
            ViewBag.TemplateName = file.FileName;
            return ExtendedJson(new
            {
                success = true,
                html = RenderVariantPartialViewToString("Dialog/FillTemplate", variables),
                fileid,
                contractid = id,
                entry = entryid
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The GenerateDocument.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="fileEntry">The fileEntry<see cref="int"/>.</param>
        /// <param name="fileid">The fileid<see cref="Guid"/>.</param>
        /// <param name="filename">The filename<see cref="string"/>.</param>
        /// <param name="variables">The variables<see cref="List{DocumentVariable}"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult GenerateDocument(int id, int fileEntry, Guid fileid, string filename, List<Guardian.Domain.Interfaces.Services.DocumentVariable> variables)
        {
            Dictionary<string, Guardian.Domain.Interfaces.Services.DocumentVariable> vars = variables?.Count > 0 ? variables.ToDictionary(v => v.Label, v => v) : new Dictionary<string, Guardian.Domain.Interfaces.Services.DocumentVariable>();
            try
            {
                DocumentGenerationResult doc = documentService.GenerateDocument(id, fileEntry, filename, vars);
                ViewBag.FileKey = doc.FileName;
                ViewBag.ContractID = id;
                FileEditModel file = fileService.GetFileByID(fileid);
                ViewBag.TemplateName = file.FileName;
                ViewBag.FileName = filename;
                ViewBag.GenerationResult = doc;
                return ExtendedJson(new
                {
                    success = true,
                    fileid,
                    filekey = doc.FileName,
                    result = doc,
                    html = RenderVariantPartialViewToString("Dialog/GeneratedDocument", doc),
                    transports = DocumentGenerationSettings.Get<string>(DocumentGenerationSettings.Transports).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogException($"Unexpected exception generating template {fileid.ToString()}", ex);
                return ExtendedJson(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The GetChangesForContract.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="ViewResult"/>.</returns>
        public ViewResult GetChangesForContract(int ID)
        {
            AgreedValueContractEditModel contract = (AgreedValueContractEditModel)contractService.GetContractEdit(ID);
            ViewBag.ContractName = contract.Description;
            LeaseAccountingReviewEditModel draftReview = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, false);
            LeaseAccountingReviewEditModel lastReview = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewForContract(ID, true);
            draftReview.ContractChangesOnSubmit = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewChanges(draftReview, lastReview);
            List<ActionGroup> actiongroups = draftReview.GetActionsForLeaseAccountingReview();
            return View("DisplayTemplates/LeaseAccountingReviewContractChangeList", actiongroups);
        }

        /// <summary>
        /// The GetCMISClassifications.
        /// </summary>
        /// <param name="term">The term<see cref="string"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult GetCMISClassifications(string term)
        {
            return ExtendedJson(documentService.GetCMISClassifications(term), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The GetCMISForm.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="classificationID">The classificationID<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult GetCMISForm(int id, int classificationID = -1)
        {
            // dont need id yet but we will
            ContractViewModel contract = contractService.GetContractView(id);
            AssetViewModel asset = assetService.GetAssetView(contract.Assets().First(), false, false, false, false, false);
            ViewBag.Classification = documentService.GetCMISClassifications("").First(c => c.ClassID == (classificationID < 1 ? ClientContext.Current.GetConfigurationSetting("CMIS.DefaultClassification", 188) : classificationID));
            ViewBag.Customer = documentService.GetCMISIndexEntries("index-11", "." + contract.ReferenceNo + ")").FirstOrDefault();
            ViewBag.Property = documentService.GetCMISIndexEntries("index-700", "." + asset.ReferenceNo + ")").FirstOrDefault();
            return PartialView("Dialog/CMISForm");
        }

        /// <summary>
        /// The GetCMISIndexEntries.
        /// </summary>
        /// <param name="parent">The parent<see cref="string"/>.</param>
        /// <param name="term">The term<see cref="string"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult GetCMISIndexEntries(string parent, string term)
        {
            return ExtendedJson(documentService.GetCMISIndexEntries(parent, term), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The GetContractAuditLogSpreadsheet.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="fromDate">The fromDate<see cref="DateTime?"/>.</param>
        /// <param name="toDate">The toDate<see cref="DateTime?"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult GetContractAuditLogSpreadsheet(int ID, DateTime? fromDate = null, DateTime? toDate = null)
        {
            IEnumerable<SystemAuditLogEntry> auditLogs = auditService.GetContractsLeaseAccountingReviewAuditEntries(new List<int> { ID }, fromDate, toDate);

            ExcelPackage package = GenerateAdminExcelLog(auditLogs);

            ContentDisposition cd = new ContentDisposition
            {
                FileName = "ContractAuditLogs_" + DateTime.Now.ToString(UserContext.Current.DateFormat) + ".xlsx",
                Inline = true,
                DispositionType = DispositionTypeNames.Attachment
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());

            Response.SetCookie(new HttpCookie("fileDownload") { Value = "true", HttpOnly = false });

            return File(package.GetAsByteArray(), "application/vnd.ms-excel");
        }

        /// <summary>
        /// The GetContractTypes.
        /// </summary>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult GetContractTypes()
        {
            List<ContractTypeEditModel> ContractTypes = contractTypeService.GetContractTypes();
            if (ContextAssetHasValue)
            {
                AssetEditModel asset = assetService.GetAssetEdit(ContextAssetID);
                if (asset.Ownership.ToUpper() != "LEASED")
                {
                    ContractTypes = ContractTypes.Where(r => !r.SubjectToLeaseAccounting).ToList();
                }
            }
            IEnumerable<string> payable = ContractTypes.Where(m => "BP".Contains(m.Direction)).GroupBy(m => m.Category)
                .Where(d => d.Any())
                .Select(d => string.Format("\"{0}\": {{ {1} }}", d.Key, string.Join(",", d.Select(ct => "\"" + ct.Name + "\":\"" + ct.ContractTypeID + "\""))));
            IEnumerable<string> receivable = ContractTypes.Where(m => "BR".Contains(m.Direction)).GroupBy(m => m.Category)
                .Where(d => d.Any())
                .Select(d => string.Format("\"{0}\": {{ {1} }}", d.Key, string.Join(",", d.Select(ct => "\"" + ct.Name + "\":\"" + ct.ContractTypeID + "\""))));
            IOrderedEnumerable<string> categories = ContractTypes.Select(m => m.Category).Distinct().OrderBy(m => m);
            IEnumerable<string> all = ContractTypes.GroupBy(m => m.Category)
                .Where(d => d.Any())
                .Select(d => string.Format("\"{0}\": {{ {1} }}", d.Key, string.Join(",", d.Select(ct => "\"" + ct.Name + "\":\"" + ct.ContractTypeID + "\""))));
            string jsonstr = "{\"categories\" : [" + string.Join(",", categories.Select(m => "\"" + m + "\""))
                + "], \"receivable\": {" + string.Join(",", receivable) + "}, \"payable\" :{" + string.Join(",", payable) + "} , \"all\" :{ " + string.Join(",", all) + " } }";
            return Content(jsonstr, "application/json");
        }

        /// <summary>
        /// The GetContractVendorHistory.
        /// </summary>
        /// <param name="contractID">The contractID<see cref="int"/>.</param>
        /// <param name="currencyID">The currencyID<see cref="int"/>.</param>
        /// <param name="terms">The terms<see cref="IList{TermEditModel}"/>.</param>
        /// <param name="VendorHistory">The VendorHistory<see cref="IList{ContractVendorHistoryEditModel}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult GetContractVendorHistory(int contractID, int currencyID, IList<TermEditModel> terms, IList<ContractVendorHistoryEditModel> VendorHistory)
        {
            ViewBag.CurrencyID = currencyID;
            terms = (terms ?? new List<TermEditModel>()).OrderBy(m => m.TermStart).ToList();
            ViewBag.ContractStart = terms[0].TermStart;
            ViewBag.ContractEnd = terms.Last().TermEnd;
            if (VendorHistory?.Count > 0)
            {
                VendorHistory.OrderBy(v => v.ValidFrom ?? DateTime.MinValue).First().ValidFrom = null;
                return PartialView("Dialog/ContractVendorHistory", VendorHistory);
            }
            if (contractID <= 0)
            {
                return PartialView("Dialog/ContractVendorHistory", new List<ContractVendorHistoryEditModel> { new ContractVendorHistoryEditModel { ContractID = contractID, ValidFrom = null } });
            }

            IList<ContractVendorHistoryEditModel> history = contractService.GetContractVendorHistory(contractID);
            return PartialView("Dialog/ContractVendorHistory", history);
        }

        /// <summary>
        /// The GetGuaranteeTypes.
        /// </summary>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult GetGuaranteeTypes()
        {
            return ExtendedJson(contractService.GetGuaranteeTypes().Select(t => new { id = t, name = t }), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The GetNewSubContractMapping.
        /// </summary>
        /// <param name="ParentContract">The ParentContract<see cref="int"/>.</param>
        /// <param name="AssetID">The AssetID<see cref="int"/>.</param>
        /// <param name="prefix">The prefix<see cref="string"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult GetNewSubContractMapping(int ParentContract, int AssetID, string prefix)
        {
            Dictionary<int, AssetViewModel> Assets = assetService.FindMatchingAssets("", new int[] { AssetID })
                .Select(a => SimpleMapper.Map<AssetListModel, AssetViewModel>(a)).ToDictionary(a => a.AssetID, a => a);
            VMSubContractMappingModel model = new VMSubContractMappingModel
            {
                ParentContract = contractService.GetContractView(ParentContract) as AgreedValueContractViewModel,
                ExistingChildAssets = Assets.Values.Where(r => r.AssetID != AssetID).ToList(),
                ParentContractID = ParentContract,
                ParentAsset = Assets[AssetID],
                ParentAssetID = AssetID
            };
            return Json(new { html = RenderVariantPartialViewToString("EditorTemplates/SubContracts/SubContractEditModelTableRow", model, null, prefix) }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The GetParentContractCandidates.
        /// </summary>
        /// <param name="subcontractid">The subcontractid<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult GetParentContractCandidates(int subcontractid)
        {
            AgreedValueContractViewModel subcontract = contractService.GetContractView(subcontractid) as AgreedValueContractViewModel;
            //_asset.
            List<AgreedValueContractListModel> candidates = new List<AgreedValueContractListModel>();
            List<int> assetsToQuery = subcontract.Assets().ToList();
            assetsToQuery.AddRange(assetService.FindMatchingAssets("", subcontract.Assets().ToArray())
                .Where(a => a.ParentID.HasValue).Select(p => p.ParentID.Value));
            foreach (int asset in assetsToQuery)
            {
                candidates.AddRange(contractService.GetAgreedValueContracts(asset, false).Where(r =>
                    !r.IsArchived
                    && r.ContractID != subcontractid
                    && !r.IsReceivable));
            }
            VMParentContractCandidatesModel model = new VMParentContractCandidatesModel
            {
                SubContract = subcontract,
                //get rid of dups
                ParentContractCandidates = candidates.GroupBy(c => c.ContractID).Select(g => g.First()).ToList()
            };
            return PartialView("Partial/SubContracts/ParentContractCandidates", model);
        }

        /// <summary>
        /// The GetParentContractCandidatesData.
        /// </summary>
        /// <param name="subcontractid">The subcontractid<see cref="int"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        public JsonResult GetParentContractCandidatesData(int subcontractid)
        {
            AgreedValueContractViewModel subcontract = contractService.GetContractView(subcontractid) as AgreedValueContractViewModel;
            //_asset.
            List<AgreedValueContractListModel> candidates = new List<AgreedValueContractListModel>();
            List<int> assetsToQuery = subcontract.Assets().ToList();
            assetsToQuery.AddRange(assetService.FindMatchingAssets("", subcontract.Assets().ToArray())
                .Where(a => a.ParentID.HasValue).Select(p => p.ParentID.Value));
            foreach (int asset in assetsToQuery)
            {
                candidates.AddRange(contractService.GetAgreedValueContracts(asset, false).Where(r =>
                    !r.IsArchived
                    && r.ContractID != subcontractid
                    && !r.IsReceivable));
            }
            VMParentContractCandidatesModel model = new VMParentContractCandidatesModel
            {
                SubContract = subcontract,
                //get rid of dups
                ParentContractCandidates = candidates.GroupBy(c => c.ContractID).Select(g => g.First()).ToList()
            };

            return Json(new { Contracts = candidates.GroupBy(c => c.ContractID).Select(g => g.First()).ToList() }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The Index.
        /// </summary>
        /// <param name="id">The id<see cref="int?"/>.</param>
        /// <returns>The <see cref="ViewResult"/>.</returns>
        public ViewResult Index(int? id)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Base))
            {
                return Unauthorized();
            }

            ViewBag.LoadAssetID = id;
            ViewBag.Title = "Contracts";
            ViewBag.LeftLocation = Url.Action("ActionPanel", "Asset");
            ViewBag.ContentLocation = this.AutomaticContentURL(id, "Overview");
            return View("TwoColumnLayout");
        }

        /// <summary>
        /// The LeaseAccountingWizard.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="landing">The landing<see cref="bool"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult LeaseAccountingWizard(int ID, bool landing = false)
        {
            if (!UserContext.Current.EvaluateAccess(true,
                TestAssetIsAccessible,
                landing ? LeaseAccountingReviewPermissions.Landing : LeaseAccountingReviewPermissions.Create))
            {
                return PartialUnauthorized();
            }

            AgreedValueContractEditModel contract = (AgreedValueContractEditModel)contractService.GetContractEdit(ID, false);
            ViewBag.AssetID = ContextAssetID;
            ViewBag.ContractID = ID;
            ViewBag.EntityID = contract.EntityID;
            ViewBag.AssetIsEditable = assetService.AssetIsEditable(ContextAssetID) && UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit);
            ViewBag.CurrencyFormat = contract.CurrencyFormat;
            ViewBag.LeaseAccountingLedgerSystems = leaseAccountingService.GetLedgerSystems();
            ViewBag.LeaseAccountingHiddenFields = ClientContext.Current.GetConfigurationSetting("LeaseAccounting.Fields.Hidden", "").Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
            ViewBag.EarlyTerminationEstimationMethods = new List<string> { "Estimate" };
            List<LeaseAccountingReviewEditModel> pastReviews = leaseAccountingService.GetPriorLeaseAccountingReviews(ID, TimeSpan.Parse(ClientContext.Current.GetConfigurationSetting("LeaseAccounting.Preview.Timespan", "-30"))).ToList();
            ViewBag.PastReviews = pastReviews.ToList();
            DateTime termStartDate = contract.Terms.Min(t => t.TermStart);
            DateTime? termEndDate = contract.Terms.Max(t => t.TermEnd);

            DateTime contractStartValidationDate = new DateTime(termStartDate.Year, termStartDate.Month, 1);
            DateTime? contractEndValidationDate = termEndDate;
            ViewBag.contractStartValidationDate = contractStartValidationDate;
            ViewBag.contractEndValidationDate = contractEndValidationDate;

            ViewBag.LeaseAccountingReviewSimplification = LeaseAccountingOptions.Get<bool>(LeaseAccountingOptions.LeaseAccountingReviewSimplification);
            if (landing)
            {
                var verificationReasons = LeaseAccountingProviderFactory.Current.VerifyContractLeaseAccountingEnabled(contract);
                //2 means we've actively declined materiality. The page says the page "Tabs/LeaseAccountingNeedsOverride" will never show again after excluding from lease accounting reporting in which case we want to show the materiality "reasons" otherwise we go to the next page
                List<string> reasons = verificationReasons.Where(r => contract.LeaseAccounting_ManualOverride == 2 || r.Key != LeaseAccountingExclusionReasons.Materiality).SelectMany(r => r.Value.Select(e => e.ErrorMessage)).ToList();
                if (reasons.Count > 0)
                {
                    return PartialView("Tabs/NotLeaseAccountingEnabled", new ContractNotLeaseAccountingSignificant { Contract = contract, Reasons = reasons });
                }
                if (contract.NeedsOverride)
                {
                    ViewBag.ThresholdAmount = string.Format(contract.CurrencyFormat,
                        decimal.Parse(ClientContext.Current.GetConfigurationSetting("LeaseAccounting.Filtering.ValueThreshold", "10000")));
                    ViewBag.ContractValue =
                        string.Format(contract.CurrencyFormat, contract.CurrentAnnualisedLeaseAccountingValue);
                    ViewBag.ContractID = ID;
                    return PartialView(@"Tabs/LeaseAccountingNeedsOverride");
                }

                // get list of prior LeaseAccounting reviews, and the current draft review if there is one
                ViewBag.CurrentDraft = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewForContract(ID, false);
                LeaseAccountingReviewEditModel lastReview = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewForContract(ID, true);
                List<VMLeaseAccountingReviewTermChange> changes = GetLeaseAccountingTermChanges(lastReview, contract);
                try
                {
                    LeaseAccountingReviewEditModel draftReview = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, false);
                    string synchstatus = "Unsynchronized";

                    // if the contract has been synchronized previously, the deal ID of the draft review will be propagated from the previous review 
                    // when the draft review was created
                    if (draftReview.DealID.HasValue)
                    {
                        synchstatus = "Synchronized";
                    }
                    ViewBag.Status = synchstatus;
                    IEnumerable<ValidationResult> error = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview((lastReview?.HasBeenExported ?? true) ? draftReview : lastReview, contract, new ValidationContext(draftReview));
                    VMLeaseAccountingReviewEditModel model = new VMLeaseAccountingReviewEditModel
                    {
                        ContractID = ID,
                        WizardPage = 0,
                        TermChanges = changes,
                        DraftContractChanges = lastReview == null ? draftReview.ContractChangesOnSubmit : LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewChanges(draftReview, lastReview),
                        SyncIssues = error.Select(e => e.ErrorMessage).ToList()
                    };
                    return PartialView("Tabs/LeaseAccountingReview", model);
                }
                catch (InvalidOperationException ioe)
                {
                    return PartialView("Tabs/LeaseAccountingReview", new VMLeaseAccountingReviewEditModel
                    {
                        ContractID = ID,
                        WizardPage = 0,
                        TermChanges = changes,
                        DraftContractChanges = new List<LeaseAccountingReviewContractChangeListModel>(),
                        SyncIssues = new List<string>(),
                        SystemIssues = new List<string> { ioe.Message }
                    });
                }
            }
            try
            {
                VMLeaseAccountingReviewEditModel tt = GetLeaseAccountingReview(contract);
                return PartialView("Tabs/LeaseAccountingReview", tt);
            }
            catch (InvalidOperationException ioe)
            {
                return PartialView("Tabs/LeaseAccountingReview", new VMLeaseAccountingReviewEditModel
                {
                    ContractID = ID,
                    WizardPage = 0,
                    TermChanges = new List<VMLeaseAccountingReviewTermChange>(),
                    DraftContractChanges = new List<LeaseAccountingReviewContractChangeListModel>(),
                    SyncIssues = new List<string>(),
                    SystemIssues = new List<string> { ioe.Message }
                });
            }
        }

        /// <summary>
        /// The VaryLease
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/></param>
        /// <returns>The <see cref="PartialViewResult"/></returns>
        public PartialViewResult VaryLease(int ID, string effectiveDate)
        {
            AgreedValueContractEditModel contract = contractService.GetContractEdit(ID, false) as AgreedValueContractEditModel;
            DateTime date = DateTime.ParseExact(effectiveDate, UserContext.Current.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);
            if (contract == null)
            {
                return PartialView("Partial/Error", new { message = "The Contract you tried to view could not be found." });
            }

            if (contract.SubjectToLeaseAccounting)
            {
                VMAgreedValueContractEditModel vm = MapAgreedValueContractToVM(contract);

                LeaseAccountingReviewEditModel lastReview = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, false);
                VaryLeaseContractEditModel model = new VaryLeaseContractEditModel
                {
                    VaryLeaseEffectiveDate = date,
                    AssetSchedule = vm.AssetSchedule,
                    BreakClauses = vm.BreakClauses,
                    ContractID = vm.ContractID,
                    ContractIsLockedDown = vm.Lifecycle_state != "In-Abstraction",
                    IsInHoldOver = vm.IsInHoldOver,
                    IsReceivable = vm.IsReceivable,
                    ReferenceNo = vm.ReferenceNo,
                    Reviews = vm.Reviews,
                    SubjectToLeaseAccounting = vm.SubjectToLeaseAccounting,
                    Terms = vm.Terms.Select((t, i) =>
                    {
                        if (i < lastReview.Terms.Count())
                        {
                            LeaseAccountingReviewTermEditModel term = lastReview.Terms.Single(lt => lt.TermStart == t.TermStart);
                            t.State = term.ExerciseState.ToString();
                        }
                        return t;
                    }).ToList(),
                    ContractedParty = vm.ContractedParty,
                    ContractedPartyID = vm.ContractedPartyID,
                    IsPartialBuilding = vm.IsPartialBuilding,
                    LeaseAccounting_LeaseType = vm.LeaseAccounting_LeaseType,
                    TreasuryApprover = vm.TreasuryApprover,
                    TreasuryApproverID = vm.TreasuryApproverID,
                    Description = vm.Description,
                    CurrencyID = vm.CurrencyID,
                    VendorHistory = vm.VendorHistory,
                    VendorID = vm.VendorID,
                    VendorName = vm.VendorName
                };

                ViewBag.AssetID = ContextAssetID;
                ViewBag.ContractIsLockedDown = vm.ContractIsLockedDown;
                ViewBag.LeaseAccountingAccountCodes = leaseAccountingService.GetAccountCodeSegments();
                ViewBag.AssetCategoryTypes = new List<string>
                {
                    "Office Administration", "Operational"
                };
                ViewBag.LeaseAccountingLeaseTypes = contractService.GetLeaseTypes().ToList();
                ViewBag.CurrencyFormat = localeService.GetCurrency(vm.CurrencyID).FormatString;
                ViewBag.VaryLease = true;
                ViewBag.ContractIsLockedDown = false;
                return PartialView("EditorTemplates/VaryLeaseContractEditModel", model);
            }
            else
            {
                return PartialView("Partial/Error", new { message = "The Contract you tried to view is not a Subject to Lease Accounting." });
            }
        }

        /// <summary>
        /// The SaveVaryLease
        /// </summary>
        /// <param name="ID">The contract ID<see cref="int"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        public ActionResult SaveVaryLease(int ID, VaryLeaseContractEditModel model = null)
        {
            if (model == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The selected lease does not exist and may have been removed by another user"
                }, JsonRequestBehavior.AllowGet);
            }
            ModelState.Clear();
            AgreedValueContractEditModel original = contractService.GetContractEdit(ID, true) as AgreedValueContractEditModel;
            VMAgreedValueContractEditModel vm = MapAgreedValueContractToVM(original);

            if (vm == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The contract does not exist and may have been removed by another user."
                }, JsonRequestBehavior.AllowGet);
            }

            vm.Reviews = model.Reviews;
            vm.AssetSchedule = model.AssetSchedule;
            vm.BreakClauses = model.BreakClauses;
            vm.ContractedParty = model.ContractedParty;
            vm.ContractedPartyID = model.ContractedPartyID;
            vm.ContractID = model.ContractID;
            vm.ContractIsLockedDown = model.ContractIsLockedDown;
            vm.CurrencyID = model.CurrencyID;
            vm.Description = model.Description;
            vm.IsInHoldOver = model.IsInHoldOver;
            vm.IsPartialBuilding = model.IsPartialBuilding;
            vm.IsReceivable = model.IsReceivable;
            vm.LeaseAccounting_LeaseType = model.LeaseAccounting_LeaseType;
            vm.ReferenceNo = model.ReferenceNo;
            vm.SubjectToLeaseAccounting = model.SubjectToLeaseAccounting;
            vm.Terms = model.Terms.Select(t => SimpleMapper.Map<TermEditModel, TermEditModel>(t)).ToList();
            vm.TreasuryApprover = model.TreasuryApprover;
            vm.TreasuryApproverID = model.TreasuryApproverID;
            vm.VendorHistory = model.VendorHistory;
            vm.VendorID = model.VendorID;
            vm.VendorName = model.VendorName;

            foreach (TermEditModel term in vm.Terms)
            {
                if (term.State != "Exercised")
                {
                    term.State = "Pending";
                }
            }

            //Set up the default AssetSchedule
            if (model.SubjectToLeaseAccounting)
            {
                if (model.AssetSchedule.Count == 0)
                {
                    AssetEditModel aEditModel = assetService.GetAssetEdit(ContextAssetID);
                    model.AssetSchedule.Add(new ContractAssetScheduleItemEditModel
                    {
                        ID = -1,
                        Asset = aEditModel.Name,
                        IsPrimaryAsset = true,
                        AssetID = ContextAssetID,
                        BusinessUnit = aEditModel.BusinessUnit,
                        BusinessUnitID = aEditModel.BusinessUnitID,
                        LegalEntity = aEditModel.LegalEntity,
                        LegalEntityID = aEditModel.LegalEntityID
                    });
                }
                model.ContractIsLockedDown = vm.Lifecycle_state != "In-Abstraction";
            }

            ContractTypeEditModel ct = contractTypeService.GetContractType(vm.ContractTypeID);
            vm.CustomFieldValues.FillCustomFieldValue();
            vm.CustomFieldValues.Where(r => r.EntityID != vm.EntityID && r.CustomField.MinimumValues < 1).Select(c => { c.Value = ""; return c; }).ToList();
            vm.ContractCategory = ct.Category;
            List<string> errors = new List<string>();
            if (vm.IsReadOnly)
            {
                return ViewContract(ID, ContextAssetID);
            }
            try
            {
                TemplateUpdateResult result = SaveAVContract(vm, "Lease is being modified using the Lease Variation method");
                errors = ModelState.SelectMany(ms => ms.Value.Errors).Select(e => e.ErrorMessage).ToList();

                if (result != null)
                {
                    //get the updated contract from db
                    AgreedValueContractEditModel contract = contractService.GetContractEdit(ID, false) as AgreedValueContractEditModel;

                    LeaseAccountingReviewEditModel draft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, false);
                    draft.LeaseAccounting_VaryLeaseEffectiveDate = model.VaryLeaseEffectiveDate;
                    draft.IsFormalLeaseAccountingReview = true;
                    draft.Comments = model.Comments;
                    //skip the first one it'll always be exercised and we don't need to change it
                    for (int i = 1; i < draft.Terms.Count; i++)
                    {
                        TermEditModel vmterm = model.Terms.OrderBy(t => t.TermStart).Skip(i).First();
                        LeaseAccountingReviewTermExerciseStates key = (LeaseAccountingReviewTermExerciseStates)Enum.Parse(typeof(LeaseAccountingReviewTermExerciseStates), vmterm.State);
                        draft.Terms.OrderBy(t => t.TermStart).Skip(i).First().ExerciseState = key;
                    }
                    ValidationContext context = new ValidationContext(draft);
                    context.Items["AllowMultiplePatterns"] = true;
                    errors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(draft, contract, context).Select(e => e.ErrorMessage).ToList();

                    if (errors.Count == 0)
                    {
                        leaseAccountingService.AddLeaseAccountingReview(draft);
                        leaseAccountingService.SetLeaseAccountingReviewState(draft, "Submitted", LeaseAccountingReview_ProcessCode.VARY_LEASE);
                        return ExtendedJson(new
                        {
                            success = true,
                            message = "Lease variation saved successfully",
                            invoicesRemoved = result.UnsubmittedInvoicesRemoved.Count,
                            batchesRemovedFrom = result.UnsubmittedInvoicesRemoved.Select(i => i.BatchID).Distinct().Count(),
                            submittedInvoicesRetained = result.SubmittedInvoicesRetained.Count
                        });

                    }
                }
                else
                {
                    errors.Add("A problem occurred attempting to update the contract.");
                }
            }
            catch (DomainValidationException dex)
            {
                //dont need to log this as this is a validation error
                errors.AddRange(dex.Errors.Select(e => e.Message));
            }
            catch (Exception ex)
            {
                //REM-764: show generic error and log error to System audit log
                errors.Add("An error occurred attempting to update the contract and has been logged. Please report this error to Lease Accelerator support if this continues to occur");
                ISystemRepository sysRepo = ServiceLocator.Current.GetInstance<ISystemRepository>();
                var changeset = Guid.NewGuid();
                var sequence = 1;
                sysRepo.AddAuditEntry(vm.EntityID, "LeaseAccountingReview", "Lease Variation", changeset, sequence++, ChangeTypes.Error, "Exception", ex.Message, ex.StackTrace);
                var current = ex;
                while (current.InnerException != null)
                {
                    current = ex.InnerException;
                    sysRepo.AddAuditEntry(vm.EntityID, "LeaseAccountingReview", "Lease Variation", changeset, sequence++, ChangeTypes.Error, "Exception", current.Message, current.StackTrace);
                }
            }
            //if there were errors then we didn't actually save - no need to revert
            if (errors.Count > 0)
            {
                //leaseAccountingService.RevertContractToLastSynchronizedReview(ID);
                contractService.UpdateContract(original, "Reversing Lease Variation for failing validation");
            }
            SetupEditViewBag(vm);
            //Check if we're locking this down or not
            if (vm.SubjectToLeaseAccounting)
            {
                vm.ContractIsLockedDown = vm.Lifecycle_state != "In-Abstraction";
            }

            return ExtendedJson(new
            {
                success = false,
                messages = errors.ToArray()
            });
        }

        /// <summary>
        /// The UpdateAsset
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        public ActionResult UpdateAsset(ContractAddressAssetScheduleItemEditModel model)
        {
            AgreedValueContractEditModel original = contractService.GetContractEdit(model.ContractAssetScheduleItem.ContractID, false) as AgreedValueContractEditModel;

            if (model == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The selected lease does not exist and may have been removed by another user"
                }, JsonRequestBehavior.AllowGet);
            }
            ModelState.Clear();
            AgreedValueContractEditModel vm = contractService.GetContractEdit(model.ContractAssetScheduleItem.ContractID, true) as AgreedValueContractEditModel;

            if (vm == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The contract does not exist and may have been removed by another user."
                }, JsonRequestBehavior.AllowGet);
            }
            AssetEditModel originalAssetModel = assetService.GetAssetEdit(model.ContractAssetScheduleItem.AssetID);
            AssetEditModel assetModel = assetService.GetAssetEdit(model.ContractAssetScheduleItem.AssetID);

            assetModel.Address.AddressID = model.Address.AddressID;
            assetModel.Address.City = model.Address.City;
            assetModel.Address.CountryID = model.Address.CountryID;
            assetModel.Address.CountryName = model.Address.CountryName;
            assetModel.Address.IsDefaultMailingAddress = model.Address.IsDefaultMailingAddress;
            assetModel.Address.LA_ID = model.Address.LA_ID;
            assetModel.Address.Line1 = model.Address.Line1;
            assetModel.Address.Line2 = model.Address.Line2;
            assetModel.Address.Longitude = model.Address.Longitude;
            assetModel.Address.Latitude = model.Address.Latitude;
            assetModel.Address.PostCode = model.Address.PostCode;
            assetModel.Address.StateAbbreviation = model.Address.StateAbbreviation;
            assetModel.Address.StateID = model.Address.StateID;

            ContractAssetScheduleItemEditModel existingModel = vm.AssetSchedule?.SingleOrDefault(r => r.AssetID == model.ContractAssetScheduleItem.AssetID);

            existingModel.AvailableForUseDate = model.ContractAssetScheduleItem.AvailableForUseDate;
            existingModel.DepreciationStartDate = model.ContractAssetScheduleItem.DepreciationStartDate;
            existingModel.UnitPrice = model.ContractAssetScheduleItem.UnitPrice;
            existingModel.GLCode = model.ContractAssetScheduleItem.GLCode;
            existingModel.CostCenter = model.ContractAssetScheduleItem.CostCenter;
            existingModel.AssetOwner = model.ContractAssetScheduleItem.AssetOwner;
            existingModel.AssetOwnerID = model.ContractAssetScheduleItem.AssetOwnerID;
            existingModel.AssetUser = model.ContractAssetScheduleItem.AssetUser;
            existingModel.AssetUserID = model.ContractAssetScheduleItem.AssetUserID;
            existingModel.BusinessUnit = model.ContractAssetScheduleItem.BusinessUnit;
            existingModel.BusinessUnitID = model.ContractAssetScheduleItem.BusinessUnitID;
            existingModel.LegalEntity = model.ContractAssetScheduleItem.LegalEntity;
            existingModel.LegalEntityID = model.ContractAssetScheduleItem.LegalEntityID;

            vm.AssetSchedule.RemoveAll(x => x.AssetID == model.ContractAssetScheduleItem.AssetID);
            vm.AssetSchedule.Add(existingModel);

            if (vm.IsReadOnly)
            {
                return ViewContract(model.ContractAssetScheduleItem.ContractID, ContextAssetID);
            }
            TemplateUpdateResult result = new TemplateUpdateResult();
            if (ModelState.IsValid)
            {
                try
                {
                    assetService.UpdateAsset(assetModel);

                    result = contractService.UpdateContract(vm, "UpdateAsset is being modified using the Update Asset method");

                }
                catch (DomainValidationException ex)
                {
                    foreach (ValidationError error in ex.Errors)
                    {
                        ModelState.AddModelError(error.Member, error.Message);
                    }
                }
                catch (DomainSecurityException dex)
                {
                    ModelState.AddModelError("security", dex.Message);
                }
            }
            var errors = ModelState.SelectMany(ms => ms.Value.Errors).Select(e => e.ErrorMessage).ToList();

            if (result != null)
            {
                //get the updated contract from db
                var contract = contractService.GetContractEdit(model.ContractAssetScheduleItem.ContractID, false) as AgreedValueContractEditModel;

                var draft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, false);
                draft.IsFormalLeaseAccountingReview = true;
                //draft.Comments = "TO-DO";

                errors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(draft, contract, new ValidationContext(draft)).Select(e => e.ErrorMessage).ToList();

                if (errors.Count == 0)
                {
                    try
                    {
                        leaseAccountingService.AddLeaseAccountingReview(draft);
                        leaseAccountingService.SetLeaseAccountingReviewState(draft, "Submitted", LeaseAccountingReview_ProcessCode.UPDATE_ASSETS);
                        return ExtendedJson(new
                        {
                            success = true,
                            message = "Update Asset saved successfully"
                        });
                    }
                    catch (DomainValidationException dex)
                    {
                        errors.AddRange(dex.Errors.Select(e => e.Message));
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.Message);
                    }
                }
            }

            //if there were errors then we didn't actually save - no need to revert
            if (errors.Count > 0 || result == null)
            {
                contractService.UpdateContract(original, "Reversing Update Asset for failing validation");
                assetService.UpdateAsset(originalAssetModel);
            }

            return ExtendedJson(new
            {
                success = false,
                messages = errors.ToArray()
            });
        }

        /// <summary>
        /// The LinkExistingSubContractToParent
        /// </summary>
        /// <param name="subcontractid">The subcontractid<see cref="int"/>.</param>
        /// <param name="contractid">The contractid<see cref="int"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [HttpPost]
        public JsonResult LinkExistingSubContractToParent(int subcontractid, int contractid)
        {
            AgreedValueContractViewModel contract = contractService.GetContractView(contractid) as AgreedValueContractViewModel;
            List<SubContractMappingEditModel> mappings = contract.Assets().Select(a => new SubContractMappingEditModel
            {
                AssetID = a,
                ContractID = subcontractid,
                ParentContractID = contractid
            }).ToList();
            contractService.UpdateSubContractMappings(mappings);
            return Json(new { success = true });
        }

        /// <summary>
        /// The MoveContract.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="NewID">The NewID<see cref="int"/>.</param>
        /// <param name="ContextID">The ContextID<see cref="int"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult MoveContract(int ID, int NewID, int ContextID)
        {
            try
            {
                TemplateUpdateResult result = contractService.MoveContract(ID, ContextAssetID, NewID);
                return ExtendedJson(new
                {
                    success = true,
                    message = "Contract successfully moved",
                    invoicesRemoved = result.UnsubmittedInvoicesRemoved.Count,
                    batchesRemovedFrom = result.UnsubmittedInvoicesRemoved.Select(i => i.BatchID).Distinct().Count(),
                    submittedInvoicesRetained = result.SubmittedInvoicesRetained.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (DomainEntityNotFoundException nfex)
            {
                return ExtendedJson(new { success = false, message = nfex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (DomainIntegrityException iex)
            {
                return ExtendedJson(new { success = false, message = iex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (DomainSecurityException sex)
            {
                return ExtendedJson(new { success = false, message = sex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return ExtendedJson(new { success = false, message = "An unexpected error occurred preventing the contract from being moved. Please try again." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The CheckContractIsExpired
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        public ExtendedJsonResult CheckContractIsExpired(int ID)
        {
            try
            {
                bool result = contractService.IsContractExpired(ID);
                if (result)
                {
                    return ExtendedJson(new
                    {
                        success = true,
                        message = "Contract is Expired "
                    }, JsonRequestBehavior.AllowGet);
                }
                return ExtendedJson(new
                {
                    success = false,
                    message = "Contract is not Expired "
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return ExtendedJson(new { success = false, message = "An unexpected error occurred. Please try again." }, JsonRequestBehavior.AllowGet);
            }
        }




        /// <summary>
        /// The UpdateLifeCycleState
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/></param>
        /// <param name="state">The state<see cref="int"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        public ExtendedJsonResult UpdateLifeCycleState(int ID, string state)
        {
            try
            {
                contractService.UpdateLifeCycleState(ID, state);
                return ExtendedJson(new
                {
                    success = true,
                    message = "Contract successfully updated and marked as " + state
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return ExtendedJson(new { success = false, message = "An unexpected error occurred preventing the contract from being updated. Please try again." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The UpdateReasonableCertainty
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/></param>
        /// <returns>The <see cref="PartialViewResult"/></returns>
        public PartialViewResult UpdateReasonableCertainty(int ID, string effectiveDate)
        {
            AgreedValueContractEditModel contract = contractService.GetContractEdit(ID, false) as AgreedValueContractEditModel;
            LeaseAccountingReviewEditModel draftLeaseAccountingReview = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, true);
            LeaseAccountingReviewEditModel previousLeaseAccountingReview = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewForContract(contract.ContractID, true, false);

            DateTime date = DateTime.ParseExact(effectiveDate, UserContext.Current.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);

            if (previousLeaseAccountingReview == null)
            {
                previousLeaseAccountingReview = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewForContract(contract.ContractID, true, true);
            }

            TermEditModel lastTerm = contract.Terms.OrderBy(t => t.TermStart).Last(t => t.State == "Exercised" || !t.IsOption);
            VMLeaseAccountingReviewEditModel model = ConvertToVMLeaseAccountingReviewEditModel(draftLeaseAccountingReview, contract, previousLeaseAccountingReview, lastTerm);
            model.VaryLeaseEffectiveDate = date;

            ViewBag.EarlyTerminationEstimationMethods = new List<string> { "Estimate" };
            ViewBag.IsUpdateReasonableCertainty = true;

            return PartialView("Tabs/WizardPages/Duration", model);
        }

        /// <summary>
        /// The ConvertToVMLeaseAccountingReviewEditModel
        /// </summary>
        /// <param name="draft">The ID<see cref="LeaseAccountingReviewEditModel"/></param>
        /// <param name="contract">The ID<see cref="AgreedValueContractEditModel"/></param>
        /// <param name="previousReview">The ID<see cref="LeaseAccountingReviewEditModel"/></param>
        /// <param name="lastTerm">The ID<see cref="TermEditModel"/></param>
        /// <returns>The <see cref="VMLeaseAccountingReviewEditModel"/></returns>
        private VMLeaseAccountingReviewEditModel ConvertToVMLeaseAccountingReviewEditModel(LeaseAccountingReviewEditModel draft, AgreedValueContractEditModel contract, LeaseAccountingReviewEditModel previousReview, TermEditModel lastTerm)
        {
            VMLeaseAccountingReviewEditModel model = new VMLeaseAccountingReviewEditModel
            {
                CreatedByUser = draft.CreatedByUser,
                CreatedByUserID = draft.CreatedByUserID,
                CreatedByUsername = draft.CreatedByUsername,
                CreatedDateTime = draft.CreatedDateTime,
                IsNew = previousReview == null,
                ContractTermStart = contract.InitialTerm().TermStart,
                ProjectedEnd = draft.ProjectTermEnd(),
                ContractHasEnded = (contract.Terms.Where(t => t.State == "Exercised" || !t.IsOption).OrderBy(t => t.TermStart).Last().TermEnd ?? DateTime.MaxValue).Date < DateTime.Today,
                DiscountRateSighted = draft.LeaseAccounting_DiscountRateSighted,
                EarlyTerminationDate = draft.TerminationDate,
                EarlyTerminationEstimatedCost = draft.EstimatedCostComponent,
                EarlyTerminationEstimationMethod = draft.EstimationMethodolgy,
                CurrentCosts = contract.CurrentReview().ActionedReview.Costs,
                CostsAndIncentives = draft.CostsAndIncentives,
                ExpectedEarlyTermination = draft.ExpectedEarlyTermination,
                LeaseAccountingReviewID = draft.LeaseAccountingReviewID,
                PreviousDiscountRate = previousReview == null ? (decimal?)null : previousReview.LeaseAccounting_DiscountRate,
                LastTermDuration = string.Format("{0:" + UserContext.Current.DateFormat + "} - {1}", lastTerm.TermStart, lastTerm.TermEnd.HasValue ? lastTerm.TermEnd.Value.ToString(UserContext.Current.DateFormat) : "Open"),
                LastTermExpiry = lastTerm.TermEnd,
                LastTermName = lastTerm.TermName,
                WizardPage = draft.LastWizardPage,
                LeaseAccounting_AccountingCode = draft.LeaseAccounting_AccountingCode,
                LeaseAccounting_OriginalPurchasePrice = draft.LeaseAccounting_OriginalPurchasePrice,
                LeaseAccounting_EOLTakeOwnership = draft.LeaseAccounting_EOLTakeOwnership,
                LeaseAccounting_InitialPrepayment = draft.LeaseAccounting_InitialPrepayment,
                LeaseAccounting_UsefulLife = draft.LeaseAccounting_UsefulLife,
                LeaseAccounting_LeaseType = contract.LeaseAccounting_LeaseType,
                LeaseAccounting_AssetCategoryType = contract.LeaseAccounting_AssetCategoryType,
                ContractorName = draft.VendorName,
                ContractDescription = draft.ContractDescription,
                ContractReferenceNo = draft.ContractReferenceNo,
                CountryName = draft.CountryName,
                CurrencyAbbreviation = draft.CurrencyAbbreviation,
                DraftProjectedDuration = draft.DraftProjectedDuration,
                LeaseAccountingStartDate = draft.LeaseAccounting_StartDate,
                LeaseAccountingLedgerSystem = draft.LeaseAccounting_LedgerSystem,
                Reviews = contract.Reviews
                    .Where(r => r.ActionedReview == null)
                    .Select(r =>
                    {
                        LeaseAccountingReviewReviewEditModel e = draft.Reviews.FirstOrDefault(r2 => r2.ReviewID == r.ReviewID) ?? new LeaseAccountingReviewReviewEditModel
                        {
                            ID = -1,
                            LeaseAccountingReviewID = draft.LeaseAccountingReviewID
                        };
                        e.ReviewDate = r.ReviewDate;
                        e.ReviewType = r.ReviewType;
                        e.Costs = r.GetLeaseAccountingSignificantCosts().Select(c => new LeaseAccountingReviewReviewCostEditModel
                        {
                            AssetID = c.AssetID,
                            AssetName = c.Asset,
                            CategoryID = c.CategoryID,
                            CategoryName = c.Category,
                            CategoryGroup = c.CategoryGroup,
                            Increase = r.ReviewType == "Fixed" ? c.YearlyAmount ?? 0M : r.ReviewType == "Fixed%" ? c.FixedPercent ?? 0M : (c.Collar ?? 0M) + (c.Plus ?? 0M),
                            ID = -1,
                            LeaseAccountingReview_ReviewID = e.ID,
                            Label = r.ReviewType == "Fixed" || r.ReviewType == "Fixed%" ? c.Label : ""
                        }).ToList();
                        return e;
                    }).ToList(),
                UnexercisedOptions = draft.Terms.Where(t => t.TermState != "Exercised" && t.TermStart != contract.Terms.OrderBy(t2 => t2.TermStart).First().TermStart).Select(t =>
                {
                    VMLeaseAccountingUnexercisedOption o = new VMLeaseAccountingUnexercisedOption
                    {
                        TermEnd = t.TermEnd,
                        File = t.FileID == null ? null : fileService.GetFileByID(t.FileID.Value),
                        ReasonablyCertainToBeExercised = t.ExerciseState == LeaseAccountingReviewTermExerciseStates.NotSelected ? (bool?)null :
                            t.ExerciseState == LeaseAccountingReviewTermExerciseStates.ReasonablyCertainToBeExercised,
                        Note = t.Journal.OrderBy(j => j.EntryDateTime).DefaultIfEmpty(new LeaseAccountingReviewTermJournalEditModel { Note = "" }).Last().Note,
                        TermName = t.TermName,
                        TermStart = t.TermStart
                    };
                    return o;
                }).ToList(),
                PreviousProjectedDuration = previousReview == null ? 0 : previousReview.ProjectWholeYears(),
            };

            return model;
        }

        /// <summary>
        /// The SaveLeaseAccountingReviewDurationTab
        /// </summary>
        /// <param name="review">The ID<see cref="LeaseAccountingReviewEditModel"/></param>
        /// <param name="sysRepo">The ID<see cref="ISystemRepository"/></param>
        /// <param name="changeSet">The ID<see cref="Guid"/></param>
        /// <param name="currency">The ID<see cref="CurrencyViewModel"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        private Tuple<bool, string> SaveLeaseAccountingReviewDurationTab(LeaseAccountingReviewEditModel review, ISystemRepository sysRepo, Guid changeSet, CurrencyViewModel currency)
        {
            // save option assumptions, notes, files, early termination fields
            int currentDuration = review.ProjectWholeYears();
            int seq = 0;
            foreach (LeaseAccountingReviewTermEditModel option in review.Terms)
            {
                if (option.ExerciseState == LeaseAccountingReviewTermExerciseStates.Exercised)
                {
                    continue;
                }

                string prefix = option.TermStart.ToString("yyyyMMdd");
                string certain = Request.Params[prefix + "_certainToBeExercised"];
                bool? bCertain = string.IsNullOrWhiteSpace(certain) ? (bool?)null : Convert.ToBoolean(certain);
                LeaseAccountingReviewTermExerciseStates newState = bCertain.HasValue ? bCertain.Value ? LeaseAccountingReviewTermExerciseStates.ReasonablyCertainToBeExercised : LeaseAccountingReviewTermExerciseStates.ReasonablyCertainNotToBeExercised : LeaseAccountingReviewTermExerciseStates.NotSelected;
                if (option.ExerciseState != newState)
                {
                    sysRepo.AddAuditEntry(For(review.EntityID, "LeaseAccounting Review", "LeaseAccounting Review", changeSet, seq++, ChangeTypes.Update,
                        string.Format("{0} {1:" + UserContext.Current.DateFormat + "} - {2}", option.TermName, option.TermStart, option.TermEnd.HasValue ? option.TermEnd.Value.ToString(UserContext.Current.DateFormat) : "Open"),
                        option.ExerciseState == LeaseAccountingReviewTermExerciseStates.Exercised ? "Exercised" : option.ExerciseState == LeaseAccountingReviewTermExerciseStates.ReasonablyCertainToBeExercised ? "Reasonably certain to be exercised" : "Reasonably certain not to be exercised",
                        newState == LeaseAccountingReviewTermExerciseStates.Exercised ? "Exercised" : newState == LeaseAccountingReviewTermExerciseStates.ReasonablyCertainToBeExercised ? "Reasonably certain to be exercised" : "Reasonably certain not to be exercised"
                    ));
                }
                option.ExerciseState = newState;

                string optionNote = string.IsNullOrWhiteSpace(Request.Params[prefix + "_optionNotes"]) ? "" : Request.Params[prefix + "_optionNotes"];
                string existingNote = option.Journal.OrderBy(j => j.EntryDateTime).DefaultIfEmpty(new LeaseAccountingReviewTermJournalEditModel { Note = "" }).Last().Note;

                if (existingNote != optionNote)
                {
                    option.Journal.Add(new LeaseAccountingReviewTermJournalEditModel
                    {
                        EntryDateTime = DateTime.Now,
                        LeaseAccountingReviewID = review.LeaseAccountingReviewID,
                        LeaseAccounting_ReviewTermID = option.ID,
                        Note = optionNote,
                        User = UserContext.Current.DisplayName,
                        UserID = UserContext.Current.UserID,
                        Username = UserContext.Current.Username
                    });
                    sysRepo.AddAuditEntry(For(review.EntityID, "LeaseAccounting Review", "LeaseAccounting Review", changeSet, seq++,
                            string.IsNullOrWhiteSpace(existingNote) ? ChangeTypes.Add : ChangeTypes.Update,
                            option.TermName + " (" + option.TermStart.ToString(UserContext.Current.DateFormat) + " - " + (option.TermEnd == null ? "Open" : option.TermEnd.Value.ToString(UserContext.Current.DateFormat)) + ")",
                            existingNote,
                            optionNote
                    ));
                }
                string fileID = Request.Params[prefix + "_File_FileID"];
                if (string.IsNullOrWhiteSpace(fileID))
                {
                    if (option.FileID == null)
                    {
                        continue;
                    }

                    FileEditModel file = fileService.GetFileByID(option.FileID.Value);
                    try
                    {
                        fileService.SaveFile(file, fileService.GetFileBlob(file), true);
                    }
                    catch (DomainSecurityException dsexc)
                    {
                        return new Tuple<bool, string>(false, "An Error occurred - " + dsexc.Message);
                    }
                    sysRepo.AddAuditEntry(For(review.EntityID, "LeaseAccounting Review", "LeaseAccounting Review", changeSet, seq++,
                            ChangeTypes.Delete,
                            option.TermName + " (" + option.TermStart.ToString(UserContext.Current.DateFormat) + " - " + (option.TermEnd == null ? "Open" : option.TermEnd.Value.ToString(UserContext.Current.DateFormat)) + ")",
                            "File: " + (file == null ? "File missing - unknown file" : file.FileName),
                            "-"
                    ));
                    option.FileID = null;
                }
                else
                {
                    FileEditModel file = fileService.GetFileByID(Guid.Parse(fileID));
                    if (option.FileID.HasValue && option.FileID != Guid.Parse(fileID))
                    {
                        FileEditModel oldFile = fileService.GetFileByID(option.FileID.Value);
                        sysRepo.AddAuditEntry(For(review.EntityID, "LeaseAccounting Review", "LeaseAccounting Review", changeSet, seq++,
                            option.FileID == null ? ChangeTypes.Add : ChangeTypes.Update,
                            option.TermName + " (" + option.TermStart.ToString(UserContext.Current.DateFormat) + " - " + (option.TermEnd == null ? "Open" : option.TermEnd.Value.ToString(UserContext.Current.DateFormat)) + ")",
                            "File: " + (oldFile == null ? "File missing - unknown file" : oldFile.FileName),
                            "File: " + file.FileName
                        ));
                    }
                    option.FileID = Guid.Parse(fileID);
                    try
                    {
                        fileService.SaveFile(file, fileService.GetFileBlob(file), true);
                    }
                    catch (DomainSecurityException dsexc)
                    {
                        return new Tuple<bool, string>(false, "An Error occurred - " + dsexc.Message);
                    }
                }
            }
            bool newExpected = Convert.ToBoolean(Request.Params["expectedEarlyTermination"] ?? "false");
            decimal estimatedCost = 0M;
            bool validDate = DateTime.TryParseExact(Request.Params["EarlyTerminationDate"], UserContext.Current.DateFormat, CultureInfo.CurrentUICulture, DateTimeStyles.AssumeLocal, out DateTime temp);
            if (!validDate && newExpected)
            {
                return new Tuple<bool, string>(false, "Expected early termination is set,however an expected termination date has not been supplied");
            }

            if (newExpected)
            {
                if (!decimal.TryParse(Request.Params["estimatedCostComponent"], out estimatedCost))
                {
                    return new Tuple<bool, string>(false, "The provided estimated cost component is not valid");
                }
                if (validDate && review.TermStart() > temp)
                {
                    return new Tuple<bool, string>(false, "The provided Early Termination Date occurs before the start of the Contract");
                }
                if (review.Terms.Any(t => t.TermStart.Date > temp.Date && t.ExerciseState == LeaseAccountingReviewTermExerciseStates.ReasonablyCertainToBeExercised))
                {
                    return new Tuple<bool, string>(false, "Expected early termination is set, however at least one option is set as reasonably certain to be exercised with a start date after the termination date");
                }
            }

            if (review.Terms.Count > 0)
            {
                foreach (var term in review.Terms)
                {
                    if (!term.TermEnd.HasValue && term.ExerciseState == LeaseAccountingReviewTermExerciseStates.ReasonablyCertainToBeExercised)
                    {
                        return new Tuple<bool, string>(false, "The Month-to-Month option cannot have a status of 'Reasonably certain to be exercised' as per contractual requirements. Please update the status of the Month-to-Month option accordingly.");
                    }
                }
            }

            if (review.ExpectedEarlyTermination != newExpected)
            {
                sysRepo.AddAuditEntry(For(review.EntityID, "LeaseAccounting Review", "LeaseAccounting Review", changeSet, seq++, ChangeTypes.Update,
                    "Expected Early Termination", review.ExpectedEarlyTermination ?
                            string.Format("Yes, on {0}, estimated cost component: {1}", review.TerminationDate == null ? "not set" : review.TerminationDate.Value.ToString(UserContext.Current.DateFormat), string.Format(currency.FormatString, review.EstimatedCostComponent)) :
                            "No",
                    newExpected ?
                            string.Format("Yes, on {0}, estimated cost component: {1}", temp == DateTime.MinValue ? "not set" : temp.ToString(UserContext.Current.DateFormat), string.Format(currency.FormatString, estimatedCost)) :
                            "No"
                ));
            }
            review.EstimatedCostComponent = newExpected ? estimatedCost : 0.00M;
            review.TerminationDate = newExpected ? temp == DateTime.MinValue ? (DateTime?)null : temp : null;
            review.ExpectedEarlyTermination = newExpected;
            review.EstimationMethodolgy = newExpected ? Request.Params["EarlyTerminationEstimationMethod"] ?? "" : "";
            if (currentDuration != review.ProjectWholeYears())
            {
                review.LeaseAccounting_DiscountRateSighted = false;
            }
            return new Tuple<bool, string>(true, "The contract's reasonably certainty updated successfully");
        }

        /// <summary>
        /// The SaveReasonableCertainty
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        public ExtendedJsonResult SaveReasonableCertainty(int ID, string effectiveDate)
        {
            //if (model == null)
            //{
            //    return ExtendedJson(new
            //    {
            //        success = false,
            //        message = "The selected lease does not exist and may have been removed by another user"
            //    }, JsonRequestBehavior.AllowGet);
            //}

            AgreedValueContractEditModel contract = contractService.GetContractEdit(ID, false) as AgreedValueContractEditModel;
            LeaseAccountingReviewEditModel review = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, true, true);

            DateTime date = DateTime.ParseExact(effectiveDate, UserContext.Current.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal);

            review.IsFormalLeaseAccountingReview = true;
            review.LeaseAccounting_VaryLeaseEffectiveDate = date;// model.VaryLeaseEffectiveDate;

            List<string> validationErrors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(review, contract, new ValidationContext(review)).Select(e => e.ErrorMessage).ToList();
            if (validationErrors.Count > 0)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = validationErrors.Distinct()
                });
            }

            ISystemRepository sysRepo = ServiceLocator.Current.GetInstance<ISystemRepository>();
            Guid changeSet = Guid.NewGuid();

            CurrencyViewModel currency = localeService.GetCurrency(contract.CurrencyID);

            Tuple<bool, string> result = SaveLeaseAccountingReviewDurationTab(review, sysRepo, changeSet, currency);
            if (!result.Item1)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = result.Item2
                }, JsonRequestBehavior.AllowGet);
            }
            review.Comments = "Real Estate: Update Reasonable Certainty";
            if (!TrySave(() => leaseAccountingService.UpdateLeaseAccountingReview(review)))
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "Unable to update the Contract's reasonably certainty, please try again"
                }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                leaseAccountingService.SetLeaseAccountingReviewState(review, "Submitted", LeaseAccountingReview_ProcessCode.UPDATE_RC);

                return ExtendedJson(new
                {
                    success = true,
                    message = "Contract's reasonably certainty successfully updated"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (LeaseAcceleratorImportValidationException lex)
            {
                EventLogHelper.LogException("Failed to update Lease Accounting Readiness Review state to be submitted", lex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(lex);
                var err = string.Join(", ", lex.Errors);
                if (string.IsNullOrEmpty(err))
                {
                    err = lex.Message;
                }
                return ExtendedJson(new
                {
                    success = false,
                    message = err
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                EventLogHelper.LogException("Failed to update Lease Accounting Readiness Review state to be submitted", ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return ExtendedJson(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The confirm termination.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public ExtendedJsonResult ConfirmTermination(int id)
        {
            LeaseAccountingReviewEditModel model = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewForContract(id, null, true);
            AgreedValueContractEditModel contract = contractService.GetContractEdit(id, false) as AgreedValueContractEditModel;
            ViewBag.CurrencyFormat = contract.CurrencyFormat;
            if (!model.ExpectedEarlyTermination)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The contract is not set for early termination, please update the reasonable certainly for the contract first."
                }, JsonRequestBehavior.AllowGet);
            }
            if ((model.TerminationDate ?? DateTime.MinValue) == DateTime.MinValue)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The contract does not have an early termination date set, please update the reasonable certainly for the contract first."
                }, JsonRequestBehavior.AllowGet);
            }
            return ExtendedJson(
                new
                {
                    success = true,
                    html = RenderVariantPartialViewToString("Dialog/ConfirmTermination", model)
                }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The save lease termination.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult SaveTermination(int id)
        {
            AgreedValueContractEditModel contract = contractService.GetContractEdit(id, false) as AgreedValueContractEditModel;
            LeaseAccountingReviewEditModel review = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, true, true);
            review.IsFormalLeaseAccountingReview = true;
            if (!TrySave(() => leaseAccountingService.UpdateLeaseAccountingReview(review)))
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "Unable to record contract termination, please try again"
                }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                leaseAccountingService.SetLeaseAccountingReviewState(review, "Submitted", LeaseAccountingReview_ProcessCode.RECORD_TERMINATION);

                return ExtendedJson(new
                {
                    success = true,
                    message = "Contract termination successfully recorded"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                EventLogHelper.LogException("Failed to LAR state to submitted", ex);
                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                return ExtendedJson(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// The MoveContractDialog
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="ContextID">The ContextID<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult MoveContractDialog(int ID, int ContextID)
        {
            ContractViewModel contract = contractService.GetContractView(ID);
            if (contract == null)
            {
                return ExtendedJson(new { success = false, message = "The contract does not exist and may have been removed by another user" }, JsonRequestBehavior.AllowGet);
            }

            if (contract.Assets().Any(a => a != ContextID))
            {
                return ExtendedJson(new { success = false, message = "The contract is currently linked to a different asset and cannot be moved" }, JsonRequestBehavior.AllowGet);
            }

            if (!assetService.AssetIsEditable(ContextID))
            {
                return PartialUnauthorized();
            }

            ViewBag.ID = ID;
            ViewBag.ContextID = ContextID;
            return PartialView("Dialog/MoveContractDialog");
        }

        /// <summary>
        /// The NextAVReviewView.
        /// </summary>
        /// <param name="ContractID">The ContractID<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult NextAVReviewView(AgreedValueContractViewModel AVContractModel)
        {
            AgreedValueContractViewModel contract = AVContractModel;
            int ContractID = contract.ContractID;
            AgreedValueReviewViewModel review = contract.NextReview();
            DateTime currentTermEnd = contract.CurrentEndingTerm().TermEnd ?? DateTime.MaxValue;

            bool hasLast_Acct_Approved_or_Rejected = !contract.SubjectToLeaseAccounting;
            if (contract.SubjectToLeaseAccounting)
            {
                List<LeaseAccountingSyncStatusModel> LeaseAccountingSyncStatus =
                    leaseAccountingService.GetLeaseAccountingReviewSynchronisationStatusByContract(ContractID);

                if (LeaseAccountingSyncStatus.Count > 0)
                {
                    LeaseAccountingSyncStatusModel LastLeaseAccountingSyncStatus =
                    LeaseAccountingSyncStatus.OrderByDescending(r => r.CreatedDate).FirstOrDefault();

                    if (LastLeaseAccountingSyncStatus != null)
                    {
                        hasLast_Acct_Approved_or_Rejected = hasLast_Acct_Approved_or_Rejected = new string[] { "ACCT_APPROVED", "RE_REJECTED", "ACCT_REJECTED" }.Contains(LastLeaseAccountingSyncStatus.LAP_EventCode);
                    }
                }
                else
                {
                    hasLast_Acct_Approved_or_Rejected = true;
                }
            }
            ViewBag.IsLast_ACCT_APPROVED_Or_REJECTED = hasLast_Acct_Approved_or_Rejected;

            NextAVReviewViewModel model = new NextAVReviewViewModel
            {
                ContractID = ContractID,
                NextReview = review,
                CurrentTermEnd = currentTermEnd,
                CanActionReview = review?.ReviewDate < currentTermEnd
            };
            ViewBag.AssetID = ContextAssetID;
            ViewBag.SubjectToLeaseAccounting = contract.SubjectToLeaseAccounting;
            return PartialView("DisplayTemplates/NextAVReviewView", model);
        }

        /// <summary>
        /// The NextOptionView.
        /// </summary>
        /// <param name="ContractID">The ContractID<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult NextOptionView(ContractViewModel ContractViewModelData)
        {
            ContractViewModel contract = ContractViewModelData;
            int ContractID = contract.ContractID;
            DateTime currentTermStart = contract.CurrentStartingTerm().TermEnd ?? DateTime.MinValue;

            AgreedValueContractEditModel original = contractService.GetContractEdit(ContractID) as AgreedValueContractEditModel;
            VMAgreedValueContractEditModel vm = MapAgreedValueContractToVM(original);
            VMAgreedValueReviewEditModel firstNotActioned = vm.Reviews.Where(r => r.State == "Pending"
            && r.ReviewDate.Month == currentTermStart.Month).FirstOrDefault();
            if (firstNotActioned != null)
            {
                ViewBag.firstNotActionedReviewID = firstNotActioned.ReviewID;
            }
            bool hasLast_Acct_Approved_or_Rejected = !contract.SubjectToLeaseAccounting;

            if (contract.SubjectToLeaseAccounting)
            {
                List<LeaseAccountingSyncStatusModel> LeaseAccountingSyncStatus =
                    leaseAccountingService.GetLeaseAccountingReviewSynchronisationStatusByContract(ContractID);

                if (LeaseAccountingSyncStatus.Count > 0)
                {
                    LeaseAccountingSyncStatusModel LastLeaseAccountingSyncStatus =
                   LeaseAccountingSyncStatus.OrderByDescending(r => r.CreatedDate).FirstOrDefault();

                    if (LastLeaseAccountingSyncStatus != null)
                    {
                        hasLast_Acct_Approved_or_Rejected = hasLast_Acct_Approved_or_Rejected = new string[] { "ACCT_APPROVED", "RE_REJECTED", "ACCT_REJECTED" }.Contains(LastLeaseAccountingSyncStatus.LAP_EventCode);
                    }
                }
                else
                {
                    hasLast_Acct_Approved_or_Rejected = true;
                }
            }
            ViewBag.IsLast_ACCT_APPROVED_Or_REJECTED = hasLast_Acct_Approved_or_Rejected;
            return PartialView("DisplayTemplates/NextOptionView", contract.NextOption());
        }

        /// <summary>
        /// The ParentContractList.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ParentContractList(int ID)
        {
            AgreedValueContractEditModel contract = contractService.GetContractEdit(ID, false) as AgreedValueContractEditModel;
            AgreedValueContractViewModel contractview = contractService.GetContractView(ID) as AgreedValueContractViewModel;

            ParentContractAssetListViewModel model = new ParentContractAssetListViewModel
            {
                SubContract = contractview,
            };
            if (contract.ParentContracts.Count > 0)
            {
                AgreedValueContractViewModel parentContract = contractService.GetContractView(contract.ParentContracts[0].ParentContractID.Value) as AgreedValueContractViewModel;
                Dictionary<int, decimal> rou = GetParentContractROUTotals(contractService.GetContractEdit(parentContract.ContractID) as AgreedValueContractEditModel, ID);
                Dictionary<int, AssetListModel> parentAssets = assetService
                    .FindMatchingAssets("", parentContract.Assets().ToArray())
                    .ToDictionary(a => a.AssetID, a => a);

                List<AssetListModel> childAssets = assetService.FindMatchingAssets("", contract.Assets().ToArray());
                List<ParentContractMappingModel> parentContractMappings = parentAssets.Select(pa => new ParentContractMappingModel
                {
                    ParentAssetModel = pa.Value,
                    SubContract = contract,
                    SubContractMappings = new List<SubContractMappingEditModel>(),
                    OtherROUTotal = rou[pa.Value.AssetID]
                }).ToList();

                foreach (SubContractMappingEditModel mapping in contract.ParentContracts)
                {
                    ParentContractMappingModel matching = parentContractMappings.Find(sc => sc.ParentAssetModel.AssetID == mapping.AssetID);
                    if (matching == null)
                    {
                        matching = parentContractMappings.Find(sc => sc.ParentAssetModel.AssetID == mapping.Asset.ParentID);
                    }

                    matching.SubContractMappings.Add(mapping);
                }

                model.ParentContract = parentContract;
                model.ParentContractMappings = parentContractMappings.ToList();
                return PartialView("Partial/SubContracts/ParentContractList", model);
            }

            return PartialView("Partial/SubContracts/ParentContractList", model);
        }

        /// <summary>
        /// The ProcessAVCostsSelection.
        /// </summary>
        /// <param name="guid">The guid<see cref="string"/>.</param>
        /// <param name="reviewDate">The reviewDate<see cref="DateTime"/>.</param>
        /// <param name="currencyID">The currencyID<see cref="int"/>.</param>
        /// <param name="selection">The selection<see cref="string"/>.</param>
        /// <param name="terms">The terms<see cref="List{TermEditModel}"/>.</param>
        /// <param name="reviews">The reviews<see cref="List{VMAgreedValueReviewEditModel}"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult ProcessAVCostsSelection(string guid, DateTime reviewDate, int currencyID, string selection, List<TermEditModel> terms, List<VMAgreedValueReviewEditModel> reviews)
        {
            if (reviews.Count < 1)
            {
                return ExtendedJson(new { matched = new List<string>() });
            }

            int priority = int.MaxValue;
            if (reviews.Any(r => r.Guid == guid))
            {
                priority = reviews.First(r => r.Guid == guid).ActionedReview.Priority;
            }
            VMActionAVReviewModel lastReview = reviews.Where(r => r.ActionedReview != null).OrderBy(r => r.ActionedReview.EffectiveDate).ThenBy(r => r.ActionedReview.Priority)
                .Last(r => r.ActionedReview.EffectiveDate < reviewDate || (r.ActionedReview.EffectiveDate == reviewDate && r.ActionedReview.Priority < priority)).ActionedReview;

            VMActionAVReviewModel.VMActionAVReviewTemplateModel dummyTemplate = new VMActionAVReviewModel.VMActionAVReviewTemplateModel();

            List<VMAgreedValueContractCostEditModel> selectedCosts = JsonConvert.DeserializeObject<List<VMAgreedValueContractCostEditModel>>(selection, new LocalizedDateTimeJsonConverter()).Where(c => c.AssetID > 0).ToList();
            var costs = lastReview.Templates.SelectMany(t =>
            {
                var tcs = t.ActionedCosts.Select(c => new { Template = t, Cost = c }).ToList();
                tcs.AddRange(t.UnchangedCosts.Select(c => new { Template = t, Cost = c }));
                return tcs;
            }).ToList();
            costs.AddRange(lastReview.UnchangedTemplates.SelectMany(t =>
            {
                var tcs = t.ActionedCosts.Select(c => new { Template = t, Cost = c }).ToList();
                tcs.AddRange(t.UnchangedCosts.Select(c => new { Template = t, Cost = c }));
                return tcs;
            }));
            int actionedMin = reviews.Where(r => r.ActionedReview != null).Min(r => r.ActionedReview.Templates.DefaultIfEmpty(new VMActionAVReviewModel.VMActionAVReviewTemplateModel { InvoiceTemplateID = 0 }).Min(t => t.InvoiceTemplateID));
            int unchangedMin = reviews.Where(r => r.ActionedReview != null).Min(r => r.ActionedReview.UnchangedTemplates.DefaultIfEmpty(new VMActionAVReviewModel.VMActionAVReviewTemplateModel { InvoiceTemplateID = 0 }).Min(t => t.InvoiceTemplateID));
            ViewBag.ContractStart = terms.OrderBy(t => t.TermStart).First().TermStart;
            ViewBag.ContractEnd = terms.OrderBy(t => t.TermStart).Last().TermEnd;

            costs.AddRange(lastReview.ActionedCosts_NotInvoiced.Select(c => new { Template = dummyTemplate, Cost = c }));
            costs.AddRange(lastReview.UnactionedCosts_NotInvoiced.Select(c => new { Template = dummyTemplate, Cost = c }));
            costs.AddRange(lastReview.UnchangedCosts.Select(c => new { Template = dummyTemplate, Cost = c }));
            var matchedCosts = selectedCosts.Select(sc => costs.First(c => c.Cost.AssetID == sc.AssetID && c.Cost.CategoryID == sc.CategoryID && c.Cost.Label == sc.Label)).ToList();
            int ntid = Math.Min(0, actionedMin < unchangedMin ? actionedMin : unchangedMin);
            int ntcid = Math.Min(0, reviews.Where(r => r.ActionedReview != null).SelectMany(r => r.ActionedReview.Templates.Union(r.ActionedReview.UnchangedTemplates).SelectMany(t => t.ActionedCosts.Union(t.UnchangedCosts))).DefaultIfEmpty(new AgreedValueContractCostEditModel { CostID = 0 }).Min(c => c.CostID));
            List<VMActionAVReviewModel.VMActionAVReviewTemplateModel> templates = matchedCosts.Select(t => t.Template ?? dummyTemplate).Where(t => t != dummyTemplate).Distinct().ToList();
            List<VMAgreedValueContractCostEditModel> uninvoiced = matchedCosts.Where(t => t.Template == dummyTemplate).Select(t => t.Cost).ToList();
            templates.ForEach(t =>
            {
                t.FirstInvoiceDate = AdvanceToDate(t.FirstInvoiceDate, reviewDate, t.Frequency, t.Pattern, true);
                t.InvoiceTemplateID = --ntid;
                t.ActionedCosts.AddRange(t.UnchangedCosts);
                t.UnchangedCosts.Clear();
                t.ActionedCosts.ForEach(c =>
                {
                    c.Actioned = true;
                    c.CostID = --ntcid;
                    c.TemplateCostID = -1;
                    c.FirstPaymentDate = AdvanceToDate(c.FirstPaymentDate, t.FirstInvoiceDate, c.PaymentFrequency, c.PaymentPattern, true);
                    c.SetOld();
                });
            });
            uninvoiced.ForEach(c =>
            {
                c.Actioned = true;
                c.CostID = --ntcid;
                c.TemplateCostID = null;
                c.FirstPaymentDate = AdvanceToDate(c.FirstPaymentDate, reviewDate, c.PaymentFrequency, c.PaymentPattern, true);
                c.SetOld();
            });
            List<string> groups = invoiceService.GetAllInvoiceGroups().Where(g => !string.IsNullOrWhiteSpace(g)).ToList();
            groups.AddRange(costs.Where(c => c.Template != dummyTemplate).Select(c => c.Template.InvoiceGroup).Where(g => !string.IsNullOrWhiteSpace(g)));
            groups.Add(ClientContext.Current.GetConfigurationSetting("Invoices.DefaultGroup", "Basic Invoice"));
            groups = groups.Distinct().OrderBy(g => g, StringComparer.OrdinalIgnoreCase).ToList();
            ViewBag.InvoiceGroups = groups.Select(g => new SelectListItem { Text = g, Value = g }).ToList();
            ViewBag.InvoiceTypes = invoiceTypeService.GetInvoiceTypes().Select(t => new SelectListItem { Text = t.Name, Value = t.InvoiceTypeID.ToString() }).ToList();
            IEnumerable<CostCategoryListModel> categories = costCategoryService.GetAllCostCategories();
            Dictionary<string, string> cpiregions = contractService.GetCPIRegionList().ToDictionary(r => r.ID.ToString(), r => r.Name);

            return ExtendedJson(new
            {
                success = true,
                templates = templates.Select(t => RenderVariantPartialViewToString("Partial/ActionAVReview_InvoiceTemplate", t)).ToList(),
                uninvoiced,
                cpiregions,
                categories = categories.ToDictionary(c => c.CostCategoryID.ToString(), c => c.DisplayName()),
                assets = assetService.GetAssetSelectList(currencyID).ToList(),
                jurisdictions = localeService.GetTaxJurisdictions().Values.ToDictionary(j => j.Code, j => new
                {
                    code = j.Code,
                    name = j.Name,
                    taxrates = (IList<VMTaxRateViewModel>)null
                })
            });
        }

        /// <summary>
        /// The RemoveDocument.
        /// </summary>
        /// <param name="filekey">The filekey<see cref="string"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        public JsonResult RemoveDocument(string filekey)
        {
            try
            {
                if (System.IO.File.Exists(Path.GetTempPath() + filekey + ".def"))
                {
                    System.IO.File.Delete(Path.GetTempPath() + filekey + ".def");
                }

                if (System.IO.File.Exists(Path.GetTempPath() + filekey + ".docx"))
                {
                    System.IO.File.Delete(Path.GetTempPath() + filekey + ".docx");
                }
            }
            catch
            {
                // ignored
            }

            Directory.EnumerateFiles(Path.GetTempPath(), "generated_*.*", SearchOption.TopDirectoryOnly).ToList().ForEach(f =>
            {
                try
                {
                    if ((DateTime.Now - System.IO.File.GetCreationTime(f)).TotalDays > 2)
                    {
                        System.IO.File.Delete(f);
                    }
                }
                catch
                {
                    // ignored
                }
            });
            return ExtendedJson(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The RenderContractTypeFields.
        /// </summary>
        /// <param name="ContractTypeID">The ContractTypeID<see cref="int"/>.</param>
        /// <param name="isReceivable">The isReceivable<see cref="bool"/>.</param>
        /// <param name="CustomFieldValues">The CustomFieldValues<see cref="List{CustomFieldValueEditModel}"/>.</param>
        /// <param name="unmapped">The unmapped<see cref="List{CustomFieldValueEditModel}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult RenderContractTypeFields(int ContractTypeID, bool isReceivable, List<CustomFieldValueEditModel> CustomFieldValues, List<CustomFieldValueEditModel> unmapped)
        {
            if (ContractTypeID > 0)
            {
                IEnumerable<CustomFieldEditModel> customfieldsForContract = new List<CustomFieldEditModel>();
                ContractTypeEditModel ct = contractTypeService.GetContractType(ContractTypeID, isReceivable ? 'R' : 'P');
                List<CustomFieldGroupEditModel> customfieldgroups = extendableService.GetExtendableEntityCustomFields(ct.EntityID, "ContractType", isReceivable ? "Receivable" : "Payable");
                ExtendendableEntityCustomFields model = new ExtendendableEntityCustomFields
                {
                    CustomFieldValues = (CustomFieldValues ?? new List<CustomFieldValueEditModel>())
                       .Union(unmapped ?? new List<CustomFieldValueEditModel>()).ToList(),
                    CustomFieldGroups = customfieldgroups,
                    MissingCustomFields = customfieldsForContract.Where(m => customfieldgroups.SelectMany(g => g.Mappings.Select(map => map.CustomField)).All(cf => cf.CustomFieldID != m.CustomFieldID))
                };

                List<CustomFieldValueEditModel> missing = ct.CustomFields.Where(cf => cf.EntitySubType == (isReceivable ? "Receivable" : "Payable"))
                    .SelectMany(c => c.Mappings.Select(m =>
                        m.DefaultValue ?? MappingContext.Instance.Map<CustomFieldEditModel, CustomFieldValueEditModel>(m.CustomField)
                    ))
                    .Where(miss => !model.CustomFieldValues.Any(v => v.CustomFieldID == miss.CustomFieldID)).ToList();
                model.CustomFieldValues.AddRange(missing);
                model.UnmappedCustomFields = model.CustomFieldValues.Where(v => model.CustomFieldGroups.SelectMany(m => m.Mappings).All(m => m.CustomField.CustomFieldID != v.CustomFieldID)).ToList();
                model.CustomFieldValues.FillCustomFieldValue();
                ViewBag.Currencies = localeService.GetAllCurrencies().Select(c => new SelectListItem { Value = c.CurrencyID.ToString(), Text = c.Name }).OrderBy(c => c.Text).ToList();
                ViewBag.UnitModels = extendableService.GetMeasurementUnits().ToList();
                return PartialView("EditorTemplates/IExtendableModel", model);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// The ReplaceDocument.
        /// </summary>
        /// <param name="filekey">The filekey<see cref="string"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult ReplaceDocument(string filekey)
        {
            HttpPostedFileBase file = Request.Files["file"];
            if (file == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The replacement file did not upload successfully"
                });
            }

            try
            {
                (bool IsInValidFileExtension, string errorMessage) = fileService.ValidateFileExtension(file.FileName);
                if (IsInValidFileExtension)
                    return ExtendedJson(new { success = false, message = errorMessage }, JsonRequestBehavior.AllowGet);

                using (MemoryStream stream = new MemoryStream())
                {
                    file.InputStream.CopyTo(stream);
                    System.IO.File.WriteAllBytes(Path.GetTempPath() + filekey + ".docx", stream.ToArray());
                    return ExtendedJson(new
                    {
                        success = true,
                        message = "Temporary document successfully updated"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "Unable to replace the temporary document with the uploaded document, please try again"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The RestoreLeaseAccountingReview.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult RestoreLeaseAccountingReview(int ID)
        {
            if (!UserContext.Current.EvaluateAccess(true, TestAssetIsAccessible, LeaseAccountingReviewPermissions.Landing, LeaseAccountingReviewPermissions.Undelete))
            {
                return JsonUnauthorized();
            }

            LeaseAccountingReviewEditModel review = leaseAccountingService.GetLeaseAccountingReviewEdit(ID);
            if (review == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review does not exist"
                });
            }

            if (review.State != LeaseAccountingConstants.LeaseAccountingStates.Deleted)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "This " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review is not flagged as deleted"
                });
            }

            if (TrySave(() =>
            {
                leaseAccountingService.RestoreLeaseAccountingReview(ID);
            }))
            {
                return ExtendedJson(new
                {
                    success = true,
                    message = "The " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review has been successfully restored",
                    row = RenderVariantPartialViewToString("Tabs/WizardPages/Partial/LeaseAccountingReviewRow", leaseAccountingService.GetLeaseAccountingReviewEdit(ID))
                });
            }

            return ExtendedJson(new
            {
                sucess = false,
                message = "An error occured restoring this review"
            });
        }

        /// <summary>
        /// The revert to lease accounting review state.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult RevertToLeaseAccountingReviewState(int ID)
        {
            if (!LeaseAccountingOptions.Get<bool>(LeaseAccountingOptions.AllowRevertToLeaseAccountingReviewState))
                RedirectToError("Cannot perform action, System configuration not set");
            try
            {
                leaseAccountingService.RevertContractToLeaseAccountingReview(ID);

                return ExtendedJson(new
                {
                    success = true,
                    message = "The " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review has been successfully restored",
                    row = RenderVariantPartialViewToString("Tabs/WizardPages/Partial/LeaseAccountingReviewRow", leaseAccountingService.GetLeaseAccountingReviewEdit(ID))
                });
            }
            catch (Exception ex)
            {
                EventLogHelper.LogException("Error reverting review", ex);
                return ExtendedJson(new
                {
                    sucess = false,
                    message = "An error occured trying to revert contract to this review"
                });
            }
            //throw new NotImplementedException();

        }

        /// <summary>
        /// Revert a review that has been actioned including removing or unlinking the invoice
        /// template associated with it.
        /// </summary>
        /// <param name="currencyID">the currently selected currency id.</param>
        /// <param name="reviews">   the list of reviews currently on the contract.</param>
        /// <param name="terms">     the list of terms currently on the contract.</param>
        /// <param name="guid">      the guid of the review to revert.</param>
        /// <returns>.</returns>
        [HttpPost]
        public ExtendedJsonResult RevertActionedAVReview(int currencyID, List<VMAgreedValueReviewEditModel> reviews, List<TermEditModel> terms, string guid)
        {
            ModelState.Clear();
            ViewBag.CurrencyFormat = localeService.GetCurrency(currencyID).FormatString;
            reviews = (reviews ?? new List<VMAgreedValueReviewEditModel>()).OrderBy(r => r.ReviewDate).ToList();

            VMAgreedValueReviewEditModel review = reviews.Find(r => r.Guid == guid);
            if (review == null)
            {
                return ExtendedJson(new { success = false, message = "Unable to revert actioned review. Review could not be found" });
            }

            if (review.ActionedReview == null)
            {
                return ExtendedJson(new { success = false, message = "Unable to revert actioned review. Review is not actioned" });
            }

            List<string> remove = review.ActionedReview.ActionedCosts_NotInvoiced.Where(c => !string.IsNullOrWhiteSpace(c.TemplateGuid) && (review.ReviewType == "Commencing" || review.ReviewType == "Adjustment"
                || review.ReviewType.StartsWith("Fixed") && review.Costs.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Label == c.Label)
                || review.Costs.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID))).Select(c => c.TemplateGuid).ToList();

            Dictionary<string, string> enddates = new Dictionary<string, string>();

            //Save the old actioned review if it exists
            review.OldActionedReview = review.ActionedReview;
            review.ActionedReview = null;
            review.State = "Pending";
            return ExtendedJson(new { success = true, row = RenderVariantPartialViewToString("DisplayTemplates/AgreedValueReviewEditModel", review), remove, enddates });
        }

        /// <summary>
        /// Revert an actioned RB review to it's unactioned state.
        /// </summary>
        /// <param name="reviews">list of reviews currently on the contract.</param>
        /// <param name="guid">   guid of the review to action.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult RevertActionedRBReview(List<VMRateReviewEditModel> reviews, string guid)
        {
            reviews = (reviews ?? new List<VMRateReviewEditModel>()).OrderBy(r => r.ReviewDate).ToList();
            VMRateReviewEditModel review = reviews.SingleOrDefault(r => r.Guid == guid);
            if (review == null)
            {
                return ExtendedJson(new { success = false, message = "The review does not exist and cannot be reverted. Please try again" });
            }

            review.ActionedReview = null;
            review.State = "Pending";
            return ExtendedJson(new
            {
                success = true,
                html = RenderVariantPartialViewToString("DisplayTemplates/RateReviewEditModel", review)
            });
        }

        /// <summary>
        /// The ReviewTypes.
        /// </summary>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult ReviewTypes()
        {
            return ExtendedJson(new[] {
                "Fixed",
                "Fixed%",
                "Market",
                "CPI"
            }.Select(r => new { id = r, name = r }).ToList(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Save the actioned review information for commencing costs, adjustment, or an actioned
        /// fixed/market/cpi review and return the html partial for display as a table row.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="terms">     list of terms currently on the contract</param>
        /// <param name="currencyID">currency ID selected on contract</param>
        /// <param name="reviews">   list of reviews currently on the contract</param>
        /// <param name="ParentContracts">The ParentContracts<see cref="List{VMParentContractsModel}"/></param>
        /// <param name="AssetSchedule">The AssetSchedule<see cref="List{ContractAssetScheduleItemEditModel}"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        [HttpPost]
        public ExtendedJsonResult SaveActionedAVReview(VMActionAVReviewModel model, List<TermEditModel> terms, int currencyID, List<VMAgreedValueReviewEditModel> reviews, List<VMParentContractsModel> ParentContracts, List<ContractAssetScheduleItemEditModel> AssetSchedule)
        {
            ViewBag.ContextID = ContextAssetID;
            ViewBag.VaryLease = Request["VaryLease"] == "True";

            Dictionary<string, string> assets = assetService.GetAssetSelectList(null).ToDictionary(a => a.Key, a => a.Name);
            if (ParentContracts != null)
            {
                ParentContracts.SelectMany(pc => pc.SubContractMappings
                    .Select(c => c.ChildAssetDetails)).ToList()
                    .ForEach(a =>
                    {
                        assets.Add(a.ID.ToString(), a.Name);
                    });
            }
            IEnumerable<SelectItem> assetselectList = assets.Select(kvp => new SelectItem { Key = kvp.Key, Name = kvp.Value, Visible = true });
            foreach (VMAgreedValueContractCostEditModel cost in model.ActionedCosts_NotInvoiced)
            {
                if (assets.ContainsKey(cost.AssetID.ToString()))
                {
                    cost.Asset = assets[cost.AssetID.ToString()];
                }
            }

            foreach (VMAgreedValueContractCostEditModel cost in model.UnactionedCosts_NotInvoiced)
            {
                if (assets.ContainsKey(cost.AssetID.ToString()))
                {
                    cost.Asset = assets[cost.AssetID.ToString()];
                }
            }
            foreach (VMActionAVReviewModel.VMActionAVReviewTemplateModel temp in model.Templates)
            {
                foreach (VMAgreedValueContractCostEditModel tempCost in temp.ActionedCosts)
                {
                    if (assets.ContainsKey(tempCost.AssetID.ToString()))
                    {
                        tempCost.Asset = assets[tempCost.AssetID.ToString()];
                    }
                }
            }
            //get
            Dictionary<int, string> categories = costCategoryService.GetAllCostCategories().ToDictionary(c => c.CostCategoryID, c => c.DisplayName());
            Dictionary<string, string> cpiregions = contractService.GetCPIRegionList().ToDictionary(r => r.ID.ToString(), r => r.Name);
            List<string> groups = invoiceService.GetAllInvoiceGroups().Where(g => !string.IsNullOrWhiteSpace(g)).ToList();
            groups.AddRange(model.Templates.Select(t => t.InvoiceGroup).Where(g => !string.IsNullOrWhiteSpace(g)));
            groups.Add(ClientContext.Current.GetConfigurationSetting("Invoices.DefaultGroup", "Basic Invoice"));
            groups = groups.Distinct().OrderBy(g => g, StringComparer.OrdinalIgnoreCase).ToList();
            ViewBag.InvoiceGroups = groups.Select(g => new SelectListItem { Text = g, Value = g }).ToList();
            ViewBag.InvoiceTypes = invoiceTypeService.GetInvoiceTypes().Select(t => new SelectListItem { Text = t.Name, Value = t.InvoiceTypeID.ToString() }).ToList();

            reviews = reviews ?? new List<VMAgreedValueReviewEditModel>();
            VMAgreedValueReviewEditModel review = null;
            reviews.Where(r => r.Guid == model.Guid).ToList().ForEach(r => { review = r; reviews.Remove(r); });
            reviews.Sort((a, b) =>
            {
                if (a.ActionedReview == null)
                {
                    if (b.ActionedReview == null)
                    {
                        return a.ReviewDate.CompareTo(b.ReviewDate);
                    }
                    return a.ReviewDate.AddSeconds(1).CompareTo(b.ActionedReview.EffectiveDate);
                }
                if (b.ActionedReview == null)
                {
                    return a.ActionedReview.EffectiveDate.CompareTo(b.ReviewDate.AddSeconds(1));
                }
                return a.ActionedReview.EffectiveDate.Date.AddSeconds(a.ActionedReview.Priority).CompareTo(b.ActionedReview.EffectiveDate.Date.AddSeconds(b.ActionedReview.Priority));
            });
            terms = terms ?? new List<TermEditModel>();
            if (terms.Count < 1)
            {
                return GetUnsuccessExtendedJsonResultForSaveActionedReview(null, "An initial term must be added to the contract before costs and reviews can be defined", model, assetselectList, categories, cpiregions);
            }

            DateTime contractStart = terms.OrderBy(t => t.TermStart).First().TermStart;

            if (model.ReviewType != "Commencing" && reviews.All(r => r.ReviewType != "Commencing"))
            {
                return GetUnsuccessExtendedJsonResultForSaveActionedReview(null, "Commencing cost must be added to the contract before actioning a review or adding a cost adjustment", model, assetselectList, categories, cpiregions);
            }

            if (model.Templates.Any(r => r.TemplateVendorID == 0))
            {
                return GetUnsuccessExtendedJsonResultForSaveActionedReview("Vendor", "Vendor cannot be null, select a vendor", model, assetselectList, categories, cpiregions);
            }

            if (model.ReviewType != "Commencing")
            {
                // look for any actioned reviews (non-costadjustment) that already exist after this review
                if (model.ReviewType != "Adjustment" && reviews.Any(r => r.ActionedReview != null && r.ReviewType != "Adjustment" && (r.ActionedReview.EffectiveDate > model.EffectiveDate || (r.ActionedReview.EffectiveDate == model.EffectiveDate && r.ActionedReview.Priority > model.Priority))))
                {
                    return GetUnsuccessExtendedJsonResultForSaveActionedReview(null, "Only the last actioned review may be edited and only reviews after the last actioned review may be actioned", model, assetselectList, categories, cpiregions);
                }
                VMAgreedValueReviewEditModel lastActionedReview = reviews.Last(r => r.ReviewType != "Adjustment" && r.ActionedReview != null);

                if (model.ReviewType != "Adjustment" && model.EffectiveDate < lastActionedReview.ActionedReview.EffectiveDate)
                {
                    return GetUnsuccessExtendedJsonResultForSaveActionedReview(null, "Actioned review effective date must be on or after the effective date of the previous actioned review (min: " + lastActionedReview.ActionedReview.EffectiveDate.ToString(UserContext.Current.DateFormat) + ")", model, assetselectList, categories, cpiregions);
                }
            }
            else
            {
                model.ActionedDate = contractStart;
                model.EffectiveDate = contractStart;
            }

            //TODO: this chunk that reforms the model.xxxCosts pieces from the request params, should probably go before any of the above `return ExtendedJson(...model...` otherwise your costs go missing if there's any errors checked above here. (but probably not the part that trims the empty templates  if you're still editing, you might still want that; we only trim it as part of saving)
            model.ActionedCosts_NotInvoiced = JsonConvert.DeserializeObject<List<VMAgreedValueContractCostEditModel>>(Request.Params["Costs"] ?? "[]", new LocalizedDateTimeJsonConverter());
            model.RemovedCosts = JsonConvert.DeserializeObject<List<VMAgreedValueContractCostEditModel>>(Request.Params["Removed"] ?? "[]", new LocalizedDateTimeJsonConverter());
            model.Templates.ForEach(t =>
            {
                t.ActionedCosts = JsonConvert.DeserializeObject<List<VMAgreedValueContractCostEditModel>>(Request.Params["Templates[" + t.Guid + "].ActionedCosts"] ?? "[]", new LocalizedDateTimeJsonConverter());
                t.UnchangedCosts = JsonConvert.DeserializeObject<List<VMAgreedValueContractCostEditModel>>(Request.Params["Templates[" + t.Guid + "].UnchangedCosts"] ?? "[]", new LocalizedDateTimeJsonConverter());
                t.ActionedCosts = t.ActionedCosts.ToList();
                t.UnchangedCosts = t.UnchangedCosts.ToList();
                t.VendorID = t.TemplateVendorID;
                t.VendorName = t.TemplateVendorName;
            });
            model.Templates = model.Templates.Where(t => t.ActionedCosts.Count + t.UnchangedCosts.Count > 0).ToList();
            //model.ActionedCosts_NotInvoiced = model.ActionedCosts_NotInvoiced.Where(c => c.AssetID > 0).ToList();
            if (model.ReviewType != "Commencing")
            {
                if (model.EffectiveDate == contractStart)
                {
                    return GetUnsuccessExtendedJsonResultForSaveActionedReview("EffectiveDate", ReviewStartDateErrorMessage, model, assetselectList, categories, cpiregions);
                }
                // now we need to add in the unchanged templates from the previous actioned review
                VMAgreedValueReviewEditModel previousActionedReview = reviews.Last(r => r.ActionedReview != null && (r.ActionedReview.EffectiveDate < model.EffectiveDate || (r.ActionedReview.EffectiveDate == model.EffectiveDate && r.ActionedReview.Priority < model.Priority)));
                List<VMAgreedValueContractCostEditModel> allCosts = model.Templates.SelectMany(t => { List<VMAgreedValueContractCostEditModel> costs = t.ActionedCosts.ToList(); costs.AddRange(t.UnchangedCosts); return costs; }).ToList();
                allCosts.AddRange(model.UnactionedCosts_NotInvoiced);
                allCosts.AddRange(model.ActionedCosts_NotInvoiced);
                allCosts.AddRange(model.RemovedCosts);
                List<VMActionAVReviewModel.VMActionAVReviewTemplateModel> unchangedTemplates = previousActionedReview.ActionedReview.Templates
                    .Where(t => !t.ActionedCosts.Any(c => allCosts.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Label == c.Label))
                    && !t.UnchangedCosts.Any(c => allCosts.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Label == c.Label))).ToList();
                unchangedTemplates.AddRange(previousActionedReview.ActionedReview.UnchangedTemplates
                    .Where(t => !t.ActionedCosts.Any(c => allCosts.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Label == c.Label))
                    && !t.UnchangedCosts.Any(c => allCosts.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Label == c.Label))));

                model.UnchangedTemplates = unchangedTemplates.Select(t => (VMActionAVReviewModel.VMActionAVReviewTemplateModel)t.Clone()).ToList();
                model.UnchangedTemplates.ForEach(t =>
                {
                    t.UnchangedCosts.AddRange(t.ActionedCosts);
                    t.ActionedCosts.Clear();
                    t.UnchangedCosts.ForEach(c => c.Actioned = false);
                });
                List<VMAgreedValueContractCostEditModel> unchangedCosts = previousActionedReview.ActionedReview.ActionedCosts_NotInvoiced
                    .Where(c => !allCosts.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Label == c.Label)).ToList();
                unchangedCosts.AddRange(previousActionedReview.ActionedReview.UnactionedCosts_NotInvoiced
                    .Where(c => !allCosts.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Label == c.Label)));
                unchangedCosts.AddRange(previousActionedReview.ActionedReview.UnchangedCosts
                    .Where(c => !allCosts.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Label == c.Label)));

                model.UnchangedCosts = unchangedCosts.Select(c => (VMAgreedValueContractCostEditModel)c.Clone()).ToList();
                model.UnchangedCosts.ForEach(c => c.Actioned = false);

                if (model.ReviewType == "Adjustment")
                {
                    reviews.Where(r => r.ActionedReview == null && r.ReviewDate >= model.EffectiveDate).ToList().ForEach(r =>
                    {
                        switch (r.ReviewType)
                        {
                            case "Fixed":
                            case "Fixed%":
                                if (r.Costs.Any(c => model.RemovedCosts.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Label == c.Label)))
                                {
                                    ModelState.AddModelError("", @"Saving this cost adjustment removes costs set for review on a fixed review on " + r.ReviewDate.ToString(UserContext.Current.DateFormat));
                                }
                                break;

                            case "Market":
                            case "CPI":
                                if (r.Costs.Any(c => model.RemovedCosts.Any(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID)))
                                {
                                    ModelState.AddModelError("", @"Saving this cost adjustment removes costs set for review on a " + r.ReviewType.ToLower() + @" review on " + r.ReviewDate.ToString(UserContext.Current.DateFormat));
                                }
                                break;
                        }
                    });
                }
            }

            ModelState.Clear();
            TryValidateModel(model);
            int minTemplateID = reviews.Where(r => r.ActionedReview != null).Select(r => r.ActionedReview).Union(new List<VMActionAVReviewModel> { model })
                .SelectMany(r => r.Templates.Union(r.UnchangedTemplates)).DefaultIfEmpty(new VMActionAVReviewModel.VMActionAVReviewTemplateModel { InvoiceTemplateID = 0 })
                    .Min(t => t.InvoiceTemplateID);

            if (minTemplateID >= 0)
            {
                minTemplateID = -1;
            }

            if (ModelState.IsValid)
            {
                model.Templates.ForEach(t =>
                {
                    if (t.InvoiceTemplateID == 0)
                    {
                        t.InvoiceTemplateID = --minTemplateID;
                    }
                    t.ActionedCosts.ForEach(c =>
                    {
                        if (assets.ContainsKey(c.AssetID.ToString()))
                        {
                            c.Asset = assets[c.AssetID.ToString()];
                        }
                        if (categories.ContainsKey(c.CategoryID))
                        {
                            c.Category = categories[c.CategoryID];
                        }

                    });
                    t.UnchangedCosts.ForEach(c =>
                    {
                        if (assets.ContainsKey(c.AssetID.ToString()))
                        {
                            c.Asset = assets[c.AssetID.ToString()];
                        }
                        if (categories.ContainsKey(c.CategoryID))
                        {
                            c.Category = categories[c.CategoryID];
                        }
                    });
                });
                model.ActionedCosts_NotInvoiced.ForEach(c =>
                {
                    if (assets.ContainsKey(c.AssetID.ToString()))
                    {
                        c.Asset = assets[c.AssetID.ToString()];
                    }
                    if (categories.ContainsKey(c.CategoryID))
                    {
                        c.Category = categories[c.CategoryID];
                    }
                });
                if (review == null)
                {
                    switch (model.ReviewType)
                    {
                        case "Commencing":
                            review = new VMAgreedValueReviewEditModel
                            {
                                Guid = "costs",
                                ReviewDate = model.EffectiveDate,
                                ReviewType = model.ReviewType,
                                ActionedReview = model,
                                Notes = model.Notes,
                                IsNew = true,
                                State = AgreedValueReviewEditModel.ReviewStates.Actioned
                            };
                            break;

                        case "Adjustment":
                            review = new VMAgreedValueReviewEditModel
                            {
                                Guid = string.IsNullOrWhiteSpace(model.Guid) ? Guid.NewGuid().ToString() : model.Guid,
                                ReviewDate = model.EffectiveDate,
                                ReviewType = model.ReviewType,
                                Notes = model.Notes,
                                ActionedReview = model,
                                IsNew = true,
                                State = AgreedValueReviewEditModel.ReviewStates.Actioned
                            };
                            break;

                        default:
                            return ExtendedJson(new
                            {
                                success = false,
                                message = "The review could not be found. Please reload the contract and try again"
                            });
                    }
                }
                else
                {
                    review.ActionedReview = model;
                    review.Notes = model.Notes;
                }
                if (model.ReviewType == "Adjustment")
                {
                    model.CloneAllCosts().ForEach(c =>
                    {
                        c.IncludeInRollForward = !c.HasChanged();
                    });
                }
                ViewBag.CurrencyFormat = localeService.GetCurrency(currencyID).FormatString;
                review.State = "Actioned";
                // we need to store the applied tax rate for each cost into the object
                review.ActionedReview.ActionedCosts_NotInvoiced.ForEach(c =>
                {
                    c.AppliedTaxRate = localeService.GetTaxRate(c.JurisdictionCode, c.TaxRateID).MultiplierForDate(review.ActionedReview.EffectiveDate).Multiplier;
                });
                review.ActionedReview.Templates.SelectMany(t => t.ActionedCosts).ToList().ForEach(c =>
                {
                    c.AppliedTaxRate = localeService.GetTaxRate(c.JurisdictionCode, c.TaxRateID).MultiplierForDate(review.ActionedReview.EffectiveDate).Multiplier;
                });

                // we need to scan forward for cost adjustments that have costs unchanged from the
                // previous actioned review if there is one, and that have been updated on this
                // review, and update them
                List<VMAgreedValueReviewEditModel> costAdjustments = reviews.Where(r => r.ReviewType == "Adjustment" && (r.ActionedReview.EffectiveDate > review.ActionedReview.EffectiveDate || (r.ActionedReview.EffectiveDate == review.ActionedReview.EffectiveDate && r.ActionedReview.Priority > review.ActionedReview.Priority))).ToList();
                Dictionary<string, string> costAdjustmentPartials = new Dictionary<string, string>();
                if (costAdjustments.Count > 0)
                {
                    VMAgreedValueReviewEditModel previousActionedReview = reviews.LastOrDefault(r => r.ActionedReview != null && (r.ActionedReview.EffectiveDate < review.ActionedReview.EffectiveDate || (r.ActionedReview.EffectiveDate == review.ActionedReview.EffectiveDate && r.ActionedReview.Priority < review.ActionedReview.Priority)));
                    if (previousActionedReview != null)
                    {
                        List<VMAgreedValueContractCostEditModel> previousCosts = previousActionedReview.ActionedReview.CloneAllCosts();
                        List<VMAgreedValueContractCostEditModel> currentCosts = review.ActionedReview.CloneAllCosts();

                        var costDic = currentCosts.ToDictionary(c => string.Join("|", c.AssetID, c.CategoryID, c.Label), c => new { Current = c, Previous = previousCosts.Find(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Label == c.Label) });
                        costAdjustments.ForEach(adj =>
                        {
                            bool unchanged = true;
                            List<VMAgreedValueContractCostEditModel> adjustmentCosts = adj.ActionedReview.CloneAllCosts();

                            adjustmentCosts.Where(c => c.IncludeInRollForward).ToList().ForEach(adjCost =>
                            {
                                string key = string.Join("|", adjCost.AssetID, adjCost.CategoryID, adjCost.Label);
                                if (costDic.ContainsKey(key))
                                {
                                    var costPair = costDic[key];
                                    adjCost.PaymentAmount = costPair.Current.PaymentAmount;
                                    adjCost.PaymentFrequency = costPair.Current.PaymentFrequency;
                                    adjCost.PaymentPattern = costPair.Current.PaymentPattern;
                                    adjCost.JurisdictionCode = costPair.Current.JurisdictionCode;
                                    adjCost.TaxRateID = costPair.Current.TaxRateID;
                                    adjCost.TaxAmount = costPair.Current.TaxAmount;
                                    adjCost.YearlyAmount = costPair.Current.YearlyAmount;
                                    unchanged = false;
                                }
                            });
                            if (!unchanged)
                            {
                                costAdjustmentPartials.Add(adj.Guid, RenderVariantPartialViewToString("DisplayTemplates/AgreedValueReviewEditModel", adj));
                            }
                        });
                    }
                }
                review.ActionedReview.GetAllCosts().ForEach(c => c.TaxRate = localeService.GetTaxRate(c.JurisdictionCode, c.TaxRateID).Name);

                return ExtendedJson(new
                {
                    success = true,
                    row = RenderVariantPartialViewToString("DisplayTemplates/AgreedValueReviewEditModel", review),
                    update = costAdjustmentPartials.Select(a => new { guid = a.Key, row = a.Value }).ToList()
                });
            }

            return GetUnsuccessExtendedJsonResultForSaveActionedReview(null, "", model, assetselectList, categories, cpiregions);
        }

        private ExtendedJsonResult GetUnsuccessExtendedJsonResultForSaveActionedReview(string key, string errorMessage, VMActionAVReviewModel model, IEnumerable<SelectItem> assetselectList, Dictionary<int, string> categories, Dictionary<string, string> cpiregions)
        {
            if (!string.IsNullOrEmpty(key))
            {
                ModelState.AddModelError(key, errorMessage);
            }
            return ExtendedJson(new
            {
                success = false,
                message = errorMessage,
                type = model.ReviewType,
                html = RenderVariantPartialViewToString("Partial/ActionAVReview", model),
                cpiregions,
                categories = categories.ToDictionary(c => c.Key.ToString(), c => c.Value),
                assets = assetselectList,
                jurisdictions = localeService.GetTaxJurisdictions().Values.ToDictionary(j => j.Code, j => new
                {
                    code = j.Code,
                    name = j.Name,
                    taxrates = (IList<VMTaxRateViewModel>)null
                }),
                groups = invoiceService.GetAllInvoiceGroups().Union(model.Templates.Select(t => t.InvoiceGroup)).Where(g => !string.IsNullOrEmpty(g)).OrderBy(g => g).ToList(),
                invoicetypes = invoiceTypeService.GetInvoiceTypes().ToDictionary(t => t.InvoiceTypeID.ToString(), t => t.Name)
            });
        }

        /// <summary>
        /// Save the updated assets and metrics/rates for an actioned RB review.
        /// </summary>
        /// <param name="terms">     list of terms currently on the contract.</param>
        /// <param name="reviews">.</param>
        /// <param name="actioned">  the updated details of the actioned rb review.</param>
        /// <param name="guid">      guid of the review being saved.</param>
        /// <param name="currencyID">currency ID currently selected on contract.</param>
        /// <param name="costs">.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult SaveActionedRBReview(List<TermEditModel> terms, List<VMRateReviewEditModel> reviews, RateActionedReviewEditModel actioned, string guid, int currencyID, string costs)
        {
            ModelState.Clear();
            ViewBag.CurrencyFormat = localeService.GetCurrency(currencyID).FormatString;
            reviews = (reviews ?? new List<VMRateReviewEditModel>()).OrderBy(r => r.ReviewDate).ToList();
            terms = (terms ?? new List<TermEditModel>()).OrderBy(t => t.TermStart).ToList();
            ViewBag.StartDate = terms[0].TermStart;
            if (terms.Count < 1)
            {
                return ExtendedJson(new { success = false, message = "An initial term must be added to the contract before costs and reviews can be defined" });
            }

            VMRateReviewEditModel review = reviews.SingleOrDefault(r => r.Guid == guid);
            if (reviews.Count > 0 && review == null)
            {
                return ExtendedJson(new { success = false, message = "The review does not exist and cannot be actioned. Please try again" });
            }

            if (review == null)
            {
                if (guid == "costs")
                {
                    review = new VMRateReviewEditModel
                    {
                        ActionedReview = actioned,
                        Guid = "costs",
                        State = RateReviewEditModel.ReviewStates.Actioned
                    };
                }
                else
                {
                    return ExtendedJson(new { success = false, message = "The review does not exist and cannot be actioned. Please try again" });
                }
            }
            else
            {
                review.ActionedReview = actioned;
            }
            review.ActionedReview.ChargeRates = JsonConvert.DeserializeObject<List<ChargeRateEditModel>>(costs);
            if (guid == "costs")
            {
                review.ReviewDate = terms[0].TermStart;
                review.ActionedReview.ActionedDate = terms[0].TermStart;
                review.ActionedReview.EffectiveDate = terms[0].TermStart;
            }
            else
            {
                if (review.ActionedReview.ActionedDate.Date <= terms[0].TermStart.Date)
                {
                    ModelState.AddModelError("ActionedDate", @"The actioned date for the review must be after the commencement date of the contract");
                }

                if (review.ActionedReview.EffectiveDate.Date <= terms[0].TermStart.Date)
                {
                    ModelState.AddModelError("EffectiveDate", @"The effective date for the review must be after the commencement date of the contract");
                }
            }
            TryValidateModel(review);

            if (ModelState.IsValid)
            {
                return ExtendedJson(new
                {
                    success = true,
                    row = RenderVariantPartialViewToString("DisplayTemplates/RateReviewEditModel", review)
                });
            }
            ViewBag.AssetID = ContextAssetID;
            ViewBag.AssetNames = assetService.GetAssetSelectList(currencyID).ToDictionary(a => int.Parse(a.Key), a => a.Name);

            return ExtendedJson(new
            {
                success = false,
                html = RenderVariantPartialViewToString("Partial/ActionRVReview", review),
                rows = review.ActionedReview.ChargeRates,
                metrics = contractService.GetAllInUseMetricTypes()
                    .Union(reviews.Where(r => r.ActionedReview != null).SelectMany(c => c.ActionedReview.ChargeRates.Select(r => r.Metric)))
                    .Union(review.ActionedReview.ChargeRates.Select(c => c.Metric))
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList()
            });
        }

        /// <summary>
        /// The SaveAssetDetails.
        /// </summary>
        /// <param name="model">The model<see cref="ContractAssetScheduleItemEditModel"/>.</param>
        /// <param name="AssetSchedule">The AssetSchedule<see cref="List{ContractAssetScheduleItemEditModel}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult SaveAssetDetails(ContractAssetScheduleItemEditModel model, List<ContractAssetScheduleItemEditModel> AssetSchedule)
        {
            ContractAssetScheduleItemEditModel existingModel = AssetSchedule?.SingleOrDefault(r => r.AssetID == model.AssetID);
            if (existingModel == null)
            {
                int minid = AssetSchedule.Min(r => r.ID);
                if (minid >= 0)
                {
                    minid = -1;
                }

                model.ID = minid;
                AssetSchedule.Add(model);
            }
            else
            {
                existingModel.AvailableForUseDate = model.AvailableForUseDate;
                existingModel.DepreciationStartDate = model.DepreciationStartDate;
                existingModel.UnitPrice = model.UnitPrice;
                existingModel.GLCode = model.GLCode;
                existingModel.CostCenter = model.CostCenter;
                existingModel.AssetOwner = model.AssetOwner;
                existingModel.AssetOwnerID = model.AssetOwnerID;
                existingModel.AssetUser = model.AssetUser;
                existingModel.AssetUserID = model.AssetUserID;
                existingModel.BusinessUnit = model.BusinessUnit;
                existingModel.BusinessUnitID = model.BusinessUnitID;
                existingModel.LegalEntity = model.LegalEntity;
                existingModel.LegalEntityID = model.LegalEntityID;
            }
            ViewData.TemplateInfo.HtmlFieldPrefix = "AssetSchedule";
            ViewBag.AssetID = ContextAssetID;
            ViewBag.IsVaryLease = IsVaryLease;
            ModelState.Clear();
            return PartialView("DisplayTemplates/ContractAssetScheduleItemEditModelList", AssetSchedule);
        }

        /// <summary>
        /// Saves an unactioned agreed value review of type fixed, market, or cpi.
        /// </summary>
        /// <param name="review">model data for the review</param>
        /// <param name="terms">list of all terms currently on the contract</param>
        /// <param name="reviews">list of all reviews currently on the contract</param>
        /// <param name="currencyID">the currently selected currencyID</param>
        /// <param name="ParentContracts">The ParentContracts<see cref="List{VMParentContractsModel}"/></param>
        /// <param name="AssetSchedule">The AssetSchedule<see cref="List{ContractAssetScheduleItemEditModel}"/></param>
        /// <returns></returns>
        [HttpPost]
        public ExtendedJsonResult SaveAVReview(VMAgreedValueReviewEditModel review, List<TermEditModel> terms, List<VMAgreedValueReviewEditModel> reviews, int currencyID, List<VMParentContractsModel> ParentContracts, List<ContractAssetScheduleItemEditModel> AssetSchedule)
        {
            Dictionary<string, string> assets = assetService.GetAssetSelectList(currencyID).ToDictionary(a => a.Key, a => a.Name);
            if (ParentContracts != null)
            {
                ParentContracts.SelectMany(pc => pc.SubContractMappings.Select(c => c.ChildAssetDetails)).ToList().ForEach(a =>
                {
                    assets.Add(a.ID.ToString(), a.Name);
                });
            }
            IEnumerable<SelectItem> assetselectList = assets.Select(kvp => new SelectItem { Key = kvp.Key, Name = kvp.Value, Visible = true });

            ModelState.Clear();
            ViewBag.CurrencyFormat = localeService.GetCurrency(currencyID).FormatString;
            ViewBag.VaryLease = Request["VaryLease"] == "True";
            terms = (terms ?? new List<TermEditModel>()).OrderBy(t => t.TermStart).ToList();

            if (terms.Count < 1)
            {
                return ExtendedJson(new { success = false, message = "An initial term must be added to the contract before costs and reviews can be defined" });
            }

            DateTime first = terms[0].TermStart;
            if (review.ReviewDate.Date < first.Date)
            {
                ModelState.AddModelError("ReviewDate", @"The review must be after the commencement date of the contract");
            }

            if (review.ReviewDate.Date == first.Date)
            {
                ModelState.AddModelError("ReviewDate", ReviewStartDateErrorMessage);
            }
            //check remeasurementDate is valid
            if (review.RemeasurementDate?.Date < first.Date.AddDays(1))
            {
                ModelState.AddModelError("RemeasurementDate", @"The date change was known must be on or after the 2nd day of the contract");
            }
            if (review.RemeasurementDate?.Date > DateTime.Today)
            {
                ModelState.AddModelError("RemeasurementDate", @"The date change was known must be today or earlier");
            }
            if (review.RemeasurementDate?.Date > terms.Last().TermEnd)
            {
                ModelState.AddModelError("RemeasurementDate", @"The date change was known must be on or before the end of the contract");
            }
            if (review.RemeasurementDate?.Date > review.ReviewDate)
            {
                ModelState.AddModelError("RemeasurementDate", @"The date change was known must be on or before the review date");
            }

            review.Costs = !string.IsNullOrEmpty(Request.Params["costs"]) ? JsonConvert.DeserializeObject<List<AgreedValueReviewCostEditModel>>(Request.Params["costs"], new LocalizedDateTimeJsonConverter()).Where(c => c.AssetID > 0).ToList()
                    : review.Costs.Where(c => c != null).ToList();

            Dictionary<string, string> cpiregions = contractService.GetCPIRegionList().ToDictionary(r => r.ID.ToString(), r => r.Name);
            TryValidateModel(review);
            if (ModelState.IsValid)
            {
                review.IsNew = false;
                Dictionary<int, CostCategoryListModel> ccList = costCategoryService.GetAllCostCategories().ToDictionary(c => c.CostCategoryID);
                review.Costs.ForEach(c =>
                {
                    CostCategoryListModel cat = ccList[c.CategoryID];
                    c.Category = cat.Name;
                    c.CategoryGroup = cat.Group;
                    c.Asset = assets[c.AssetID.ToString()];
                    if (c.CPIRegionID > 0)
                    {
                        c.CPIRegion = cpiregions[c.CPIRegionID.ToString()];
                    }
                    c.CategoryIsLeaseAccountingSignificant = cat.LeaseAccountingSignificant;
                });
                if (review.ActionedReview != null)
                {
                    review.ActionedReview.AllCosts().ForEach(c =>
                    {
                        CostCategoryListModel cat = ccList[c.CategoryID];
                        c.Category = cat.Name;
                        c.CategoryGroup = cat.Group;
                        c.Asset = assets[c.AssetID.ToString()];
                        c.CategoryIsLeaseAccountingSignificant = cat.LeaseAccountingSignificant;
                    });
                }
                review.OldActionedReview = null;
                List<string> rows = new List<string>
                {
                    RenderVariantPartialViewToString("DisplayTemplates/AgreedValueReviewEditModel", review)
                };
                if (!review.Recurring)
                {
                    return ExtendedJson(new
                    {
                        success = true,
                        rows
                    });
                }

                for (int i = 0; i < review.Instances; i++)
                {
                    switch (review.Pattern)
                    {
                        case "Months":
                            review.ReviewDate = review.ReviewDate.AddMonths(review.Interval);
                            break;

                        case "Years":
                            review.ReviewDate = review.ReviewDate.AddYears(review.Interval);
                            break;
                    }
                    review.Guid = Guid.NewGuid().ToString();
                    rows.Add(RenderVariantPartialViewToString("DisplayTemplates/AgreedValueReviewEditModel", review));
                }
                return ExtendedJson(new
                {
                    success = true,
                    rows
                });
            }
            IEnumerable<CostCategoryListModel> categories = costCategoryService.GetAllCostCategories();

            return ExtendedJson(new
            {
                success = false,
                type = review.ReviewType,
                html = RenderVariantPartialViewToString("EditorTemplates/AgreedValueReviewEditModel", review),
                rows = review.Costs,
                cpiregions,
                categories = categories.ToDictionary(c => c.CostCategoryID.ToString(), c => c.DisplayName()),
                assets = assetselectList,
                jurisdictions = localeService.GetTaxJurisdictions().Values.ToDictionary(j => j.Code, j => new
                {
                    code = j.Code,
                    name = j.Name,
                    taxrates = (IList<VMTaxRateViewModel>)null
                })
            });
        }

        /// <summary>
        /// The SaveBreakClause.
        /// </summary>
        /// <param name="model">The model<see cref="BreakClauseEditModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult SaveBreakClause(BreakClauseEditModel model)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return JsonUnauthorized();
            }

            if (ModelState.IsValid)
            {
                return ExtendedJson(new { success = true, row = RenderVariantPartialViewToString("DisplayTemplates/BreakClauseEditModel", model) });
            }
            return PartialView("EditorTemplates/BreakClauseEditModel", model);
        }

        /// <summary>
        /// The SaveClause.
        /// </summary>
        /// <param name="contractTypeId">The contractTypeId<see cref="int"/>.</param>
        /// <param name="model">The model<see cref="ContractClauseEditModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult SaveClause(int contractTypeId, ContractClauseEditModel model)
        {
            ModelState.Clear();
            PredefinedClauseViewModel predefinedClause = contractService.GetPredefinedClauses().FirstOrDefault(c => c.Category == model.Category && c.Clause == model.Clause);
            ContractTypeEditModel contractType = contractTypeService.GetContractType(contractTypeId);
            model.IsActive = true;
            if (predefinedClause != null)
            {
                model.IsPredefinedClause = true;
                model.IsRequired = contractType.PredefinedClauses.FirstOrDefault(c => c.Category == model.Category && c.Clause == model.Clause)?.IsRequired ?? false;
                model.YearFieldMode = predefinedClause.YearFieldMode;
                model.PercentageFieldMode = predefinedClause.PercentageFieldMode;
                model.AreaFieldMode = predefinedClause.AreaFieldMode;
                model.AmountPayableMode = predefinedClause.AmountPayableMode;
                model.PayableToMode = predefinedClause.PayableToMode;
                model.AmountReceivableMode = predefinedClause.AmountReceivableMode;
                model.ReceivableFromMode = predefinedClause.ReceivableFromMode;
            }
            TryValidateModel(model);
            if (ModelState.IsValid)
            {
                return ExtendedJson(new { success = true, row = RenderVariantPartialViewToString("DisplayTemplates/ContractClauseEditModel", model) });
            }
            IEnumerable<PredefinedClauseViewModel> clauses = contractService.GetPredefinedClauses();
            ViewBag.ClauseCategories = GenerateClauseHeirarchy(model);
            return PartialView("EditorTemplates/ContractClauseEditModel", model);
        }

        /// <summary>
        /// The SaveClauseTriggeredRecord.
        /// </summary>
        /// <param name="clause">The clause<see cref="ClauseTriggeredRecordEditModel"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult SaveClauseTriggeredRecord(ClauseTriggeredRecordEditModel clause)
        {
            if (ModelState.IsValid)
            {
                return ExtendedJson(new
                {
                    success = true,
                    html = RenderVariantPartialViewToString("DisplayTemplates/ClauseTriggeredRecordEditModel", clause)
                });
            }
            else
            {
                if (clause.TriggeredOn == DateTime.MinValue)
                {
                    clause.TriggeredOn = DateTime.Today;
                }
                return ExtendedJson(new
                {
                    success = false,
                    html = RenderVariantPartialViewToString("EditorTemplates/ClauseTriggeredRecordEditModel", clause)
                });
            }
        }

        /// <summary>
        /// validateReview.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public IEnumerable<string> ValidateReview(AgreedValueContractEditModel contract)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            List<string> reasons = LeaseAccountingProviderFactory.Current.VerifyContractLeaseAccountingEnabled(contract).SelectMany(r => r.Value.Select(e => e.ErrorMessage)).ToList();
            if (reasons.Count > 0)
            {
                foreach (string reason in reasons)
                {
                    errors.Add(new ValidationResult(reason));
                }
            }
            else
            {
                if (contract.Reviews.Count > 0)
                {
                    LeaseAccountingReviewEditModel currentDraft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, false);
                    errors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(currentDraft, contract, new ValidationContext(currentDraft)).ToList();
                }
                else
                {
                    errors.Add(new ValidationResult("Contract does not contain any Reviews and Adjustments"));
                }
            }
            return errors.Select(e => e.ErrorMessage).Distinct();
        }
        /// <summary>
        /// validateReview.
        /// </summary>
        /// <param name="editModelContract">The ID<see cref="VMAgreedValueContractEditModel"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult ValidateReview(VMAgreedValueContractEditModel editModelContract)
        {
            AgreedValueContractEditModel contract = new AgreedValueContractEditModel();

            contract = SimpleMapper.Map<VMAgreedValueContractEditModel, AgreedValueContractEditModel>(editModelContract);

            Dictionary<int, CostCategoryListModel> costcategoryDictionary = costCategoryService.GetAllCostCategories().ToDictionary(c => c.CostCategoryID);

            editModelContract.Reviews.ForEach(r =>
            {
                if (r.ActionedReview != null)
                {
                    AgreedValueReviewEditModel matchingReview = contract.Reviews.First(cr => cr.ReviewID == r.ReviewID);

                    matchingReview.ActionedReview.Costs = r.ActionedReview.AllCosts().Select(ac =>
                    {
                        AgreedValueContractCostEditModel cost = SimpleMapper.Map<VMAgreedValueContractCostEditModel, AgreedValueContractCostEditModel>(ac);
                        //get the CategoryIsLeaseAccountingSignificant here because it's not loading with the data model (ActionReview)
                        cost.CategoryIsLeaseAccountingSignificant = costcategoryDictionary[cost.CategoryID].LeaseAccountingSignificant;
                        return cost;
                    }).ToList();
                }
            });

            editModelContract.Terms.First(t => t.IsOption == false).State = "Exercised";

            List<ValidationResult> errors = new List<ValidationResult>();

            List<string> reasons = LeaseAccountingProviderFactory.Current.VerifyContractLeaseAccountingEnabled(contract).SelectMany(r => r.Value.Select(e => e.ErrorMessage)).ToList();
            if (reasons.Count > 0)
            {
                foreach (string reason in reasons)
                {
                    errors.Add(new ValidationResult(reason));
                }
            }
            else
            {
                if (contract.Reviews.Count > 0)
                {
                    LeaseAccountingReviewEditModel currentDraft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, false);
                    currentDraft.VendorName = editModelContract.VendorName;
                    errors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(currentDraft, contract, new ValidationContext(currentDraft)).ToList();
                }
                else
                {
                    errors.Add(new ValidationResult("Contract does not contain any Reviews and Adjustments"));
                }
            }
            return ExtendedJson(new
            {
                success = errors.Count < 1,
                errors = errors.Select(e => e.ErrorMessage).Distinct()
            });
        }

        /// <summary>
        /// Tries to get the contract from the Request.
        /// N.B. if you're trying to update a subcontract you need to pass subcontractid in the request since it'll get overwritten when generating the View.
        /// </summary>
        /// <param name="type">.</param>
        /// <param name="journal">.</param>
        /// <param name="model">The model<see cref="VMAgreedValueContractEditModel"/>.</param>
        /// <returns>.</returns>
        [HttpPost]
        public ActionResult SaveContract(string type, string journal = "", VMAgreedValueContractEditModel model = null)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return Unauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return Unauthorized();
            }

            string ContractType;
            model.Lifecycle_state = "In-Abstraction";

            switch (type)
            {
                case "agreedvalue":

                    //Technically not correct but we're gonna just assume it's a subcontract and ignore it's implications if it's not
                    bool isSubContract = !string.IsNullOrEmpty(Request?["ParentContracts.index"]);
                    VMAgreedValueContractEditModel avcontract = model ?? (isSubContract ? new VMSubContractEditModel() : new VMAgreedValueContractEditModel());
                    if (model == null)
                    {
                        if (isSubContract)
                        {
                            TryUpdateModel((VMSubContractEditModel)avcontract);
                        }
                        else
                        {
                            TryUpdateModel(avcontract);
                        }
                    }
                    if (Request?["subcontractID"] != null)
                    {
                        avcontract.ContractID = int.Parse(Request["subcontractID"]);
                    }
                    TemplateUpdateResult result = SaveAVContract(avcontract, journal);
                    if (result != null)
                    {
                        return ExtendedJson(new
                        {
                            success = true,
                            message = string.Format("{0} saved successfully", "Agreed Value Contract"),
                            id = avcontract.ContractID,
                            invoicesRemoved = result.UnsubmittedInvoicesRemoved.Count,
                            batchesRemovedFrom = result.UnsubmittedInvoicesRemoved.Select(i => i.BatchID).Distinct().Count(),
                            submittedInvoicesRetained = result.SubmittedInvoicesRetained.Count
                        });
                    }
                    else
                    {
                        SetupEditViewBag(avcontract);
                        if (avcontract.VendorName == "- not set -" || avcontract.VendorName == "Unknown Contact")
                        {
                            ModelState.AddModelError(DisplayLabels.Landlord, String.Concat(DisplayLabels.Landlord, " must be specified before the contract can be saved"));
                        }
                        //Check if we're locking this down or not
                        if (avcontract.SubjectToLeaseAccounting)
                        {
                            avcontract.ContractIsLockedDown = !LeaseAccountingOptions.Get<bool>(LeaseAccountingOptions.ContractsAreLockDownEditable) && model.Lifecycle_state != "In-Abstraction";
                            avcontract.HasBeenSynchronized = avcontract.SubjectToLeaseAccounting && leaseAccountingService.GetPriorLeaseAccountingReviews(avcontract.ContractID, TimeSpan.MaxValue).Any(r => r.State == LeaseAccountingConstants.LeaseAccountingStates.Synchronized);
                        }

                        return PartialView("EditorTemplates/ContractEditModel", avcontract);
                    }
                case "rate":
                    {
                        ContractType = "Rate/Consumption Based Contract";
                        VMRateContractEditModel l = new VMRateContractEditModel();
                        TryUpdateModel(l);
                        l.Guarantees.ForEach(g => g.Guarantors = g.Guarantors.Distinct(new GuarantorEqualityComparer<GuarantorEditModel>()).ToList());
                        l.VendorHistory.ForEach(c => { c.ContractID = l.ContractID; });
                        if (l.Terms.Count > 0)
                        {
                            l.Terms.ForEach(t => t.IsOption = true);
                            l.Terms.First().IsOption = false;
                            l.Terms.First().State = "Exercised";
                        }
                        TryValidateModel(l);
                        if (ModelState.IsValid)
                        {
                            VMRateReviewEditModel first = l.Reviews.OrderBy(r => r.ReviewDate).First();
                            DateTime start = l.Terms.OrderBy(t => t.TermStart).First().TermStart;
                            first.ReviewDate = start;
                            first.ActionedReview.ActionedDate = start;
                            first.ActionedReview.EffectiveDate = start;
                            if (TrySave(() =>
                            {
                                if (l.ContractID < 0)
                                {
                                    contractService.CreateContract(SimpleMapper.MapNew<VMRateContractEditModel, RateValueContractEditModel>(l), journal);
                                }
                                else
                                {
                                    contractService.UpdateContract(SimpleMapper.MapNew<VMRateContractEditModel, RateValueContractEditModel>(l), journal);
                                }
                            }))
                            {
                                return ExtendedJson(new { success = true, message = string.Format("{0} saved successfully", ContractType), id = l.ContractID });
                            }
                        }
                        SetupEditViewBag(l);
                        ContractTypeEditModel ct = contractTypeService.GetContractType(l.ContractTypeID);
                        l.ContractTypeID = ct.ContractTypeID;
                        ViewBag.ContractCategory = ct.Category;
                        return PartialView("EditorTemplates/ContractEditModel", l);
                    }
                default:
                    return RedirectToAction("Dialog", "Error", new { message = "An unknown review type was returned" });
            }
        }

        /// <summary>
        /// The SaveAVContract.
        /// </summary>
        /// <param name="avcontract">The avcontract<see cref="VMAgreedValueContractEditModel"/>.</param>
        /// <param name="journal">The journal<see cref="string"/>.</param>
        /// <returns>The <see cref="TemplateUpdateResult"/>.</returns>
        private TemplateUpdateResult SaveAVContract(VMAgreedValueContractEditModel avcontract, string journal)
        {

            SystemContext.AuditLog.AddAuditEntry("Contract", "SaveAVContract", "Start", $"Saving AV Contract {avcontract.ContractID} - {avcontract.Description}, Templates {avcontract.Templates.Count}, Invoices {avcontract.Invoices.Count}");
            ModelState.Clear();
            bool isSubContract = avcontract is VMSubContractEditModel;
            //avcontract.Vendor
            avcontract.VendorHistory.ForEach(c => { c.ContractID = avcontract.ContractID; });
            avcontract.CustomFieldValues.ToList().ForEach(m => m.EntityID = avcontract.EntityID);
            avcontract.Terms.Sort((t1, t2) => t1.TermStart.CompareTo(t2.TermStart));
            avcontract.Reviews = avcontract.Reviews.Where(r => r.IsReverted == false).ToList();
            avcontract.Reviews.Sort((r1, r2) => r1.ReviewDate.CompareTo(r2.ReviewDate));
            if (avcontract.Terms.Count > 0)
            {
                avcontract.Terms.ForEach(t => t.IsOption = true);
                avcontract.Terms.First().IsOption = false;
                avcontract.Terms.First().State = "Exercised";

                DateTime termStartDate = avcontract.Terms.First().TermStart;
                if (avcontract.Reviews.Count > 1)
                {
                    DateTime ReviewStartDate = avcontract.Reviews[1].ReviewDate;
                    if (termStartDate == ReviewStartDate)
                    {
                        ModelState.AddModelError("", ReviewStartDateErrorMessage);
                    }
                }
            }
            avcontract.Guarantees.ForEach(g => g.Guarantors = g.Guarantors.Distinct(new GuarantorEqualityComparer<GuarantorEditModel>()).ToList());
            SystemContext.AuditLog.AddAuditEntry("Contract", "SaveAVContract", "Validate", $"Validating AV Contract {avcontract.ContractID} - {avcontract.Description}");

            TryValidateModel(avcontract);
            ModelState.RemoveAllForKeySuffix("].DefaultValue");
            //get rid parent contract stuff since we're not trying to
            ModelState.RemoveAllForKeyPrefix("ParentContracts[0].ParentContract");
            ModelState.RemoveAllForKeyPrefix("ParentContracts[0].SubContractMappings[0].ParentContract");
            StringLengthAttribute strLenAttr = typeof(VMContractEditModel).GetProperty("ReferenceNo").GetCustomAttributes(typeof(StringLengthAttribute), false).Cast<StringLengthAttribute>().SingleOrDefault();
            StringLengthAttribute strLenAttrContractDescription = typeof(VMContractEditModel).GetProperty("Description").GetCustomAttributes(typeof(StringLengthAttribute), false).Cast<StringLengthAttribute>().SingleOrDefault();
            if (strLenAttrContractDescription != null)
            {
                if (avcontract.Description.Length > strLenAttr.MaximumLength)
                    ModelState.AddModelError("", @"Description exceeds maximum length of " + strLenAttr.MaximumLength);
            }
            if (strLenAttr != null)
            {
                if (avcontract.ReferenceNo.Length > strLenAttr.MaximumLength)
                    ModelState.AddModelError("", @"Schedule Number exceeds maximum length of " + strLenAttr.MaximumLength);
            }
            if (ModelState.IsValid)
            {
                SystemContext.AuditLog.AddAuditEntry("Contract", "SaveAVContract", "Preparing", $"Preparing AV Contract {avcontract.ContractID} - {avcontract.Description}");
                //skip the initial actioned ones, then everything after that should be unactioned. if there's any actioned after unactioned, then we have a gap.
                if (avcontract.Reviews.OrderBy(x => x.ActionedReview?.EffectiveDate ?? x.ReviewDate).SkipWhile(x => x.ActionedReview != null).Any(x => x.ActionedReview != null))
                {
                    ModelState.AddModelError("", @"Unactioned reviews cannot be before actioned reviews. Please action the earlier pending reviews first, or remove them.");
                }

                // match up reviews and templates
                int rid = 0;
                int tcid = 0;
                int cid = 0;
                if (avcontract.BreakClauses.Count > 0)
                {
                    avcontract.BreakClauses.ForEach(b =>
                    {
                        b.BreakFeeValue = Convert.ToDecimal(b.FeeValue);
                    });
                }

                avcontract.AssetSchedule.ForEach(a => { a.ValidFrom = null; a.ValidTo = null; });

                //get term start. get the daty               
                int termstartday = avcontract.Terms.First().TermStart.Day;
                avcontract.Reviews.ForEach(r =>
                {
                    if (r.ReviewID < 1)
                    {
                        r.ReviewID = --rid;
                    }
                    if (r.ActionedReview != null)
                    {
                        r.RemeasurementDate = r.ActionedReview.RemeasurementDate; //collapse the date from an actioned review into the main review object - if it's both edited and actioned, then the only thing we care about now is the actioned details.

                        //get list of assets from avcontract.AssetSchedule and loop
                        foreach (ContractAssetScheduleItemEditModel asc in avcontract.AssetSchedule)
                        {
                            if (r.ActionedReview.AllCosts().Select(c => c.AssetID).Contains(asc.AssetID))
                            {
                                if (asc.ValidFrom == null || r.ActionedReview.EffectiveDate < asc.ValidFrom)
                                {
                                    avcontract.AssetSchedule.Where(a => a.AssetID == asc.AssetID).FirstOrDefault().ValidFrom = r.ActionedReview.EffectiveDate;
                                }

                            }
                        }

                        //remove assetes
                        foreach (int removedassetid in r.ActionedReview.RemovedCosts.Select(c => c.AssetID).Distinct())
                        {
                            if (!r.ActionedReview.AllCosts().Any(c => c.AssetID == removedassetid))
                            {
                                ContractAssetScheduleItemEditModel asc = avcontract.AssetSchedule.Where(a => a.AssetID == removedassetid).FirstOrDefault();
                                if (asc != null && (asc.ValidTo == null || r.ActionedReview.EffectiveDate > asc.ValidTo))
                                {
                                    asc.ValidTo = r.ActionedReview.EffectiveDate;
                                }
                            }
                        }

                        r.ActionedReview.ReviewID = r.ReviewID;
                        r.ActionedReviewID = r.ReviewID;
                        r.ActionedReview.Templates.ForEach(t =>
                        {
                            t.VendorID = t.TemplateVendorID;
                            t.VendorName = t.TemplateVendorName;
                            t.ActionedCosts.ForEach(c =>
                            {
                                if (c.CostID < 1)
                                {
                                    c.CostID = --cid;
                                }
                                if (c.TemplateCostID == null || c.TemplateCostID < 1)
                                {
                                    c.TemplateCostID = --tcid;
                                }
                                //check for lease accounting significant
                                if (!LeaseAccountingOptions.Get<bool>(LeaseAccountingOptions.LeaseAccountingReviewSimplification) && avcontract.SubjectToLeaseAccounting)
                                {
                                    if (DateTime.DaysInMonth(c.FirstPaymentDate.Year, c.FirstPaymentDate.Month) >= termstartday)
                                    {
                                        c.FirstPaymentDate = new DateTime(c.FirstPaymentDate.Year, c.FirstPaymentDate.Month, termstartday);
                                    }
                                    else
                                    {
                                        //default to last day of the month
                                        c.FirstPaymentDate = new DateTime(c.FirstPaymentDate.Year, c.FirstPaymentDate.Month, 1).AddMonths(1).AddDays(-1);
                                    }
                                }
                            });
                            t.UnchangedCosts.ForEach(c =>
                            {
                                if (c.CostID < 1)
                                {
                                    c.CostID = --cid;
                                }
                                if (t.InvoiceTemplateID != c.OriginalTemplateID || (c.TemplateCostID == null || c.TemplateCostID < 1))
                                {
                                    c.TemplateCostID = --tcid;
                                }
                            });
                        });
                        r.ActionedReview.ActionedCosts_NotInvoiced.ForEach(c =>
                        {
                            if (c.CostID < 1)
                            {
                                c.CostID = --cid;
                            }
                            c.TemplateCostID = null;
                        });
                        r.ActionedReview.UnactionedCosts_NotInvoiced.ForEach(c =>
                        {
                            if (c.CostID < 1)
                            {
                                c.CostID = --cid;
                            }
                            c.TemplateCostID = null;
                        });
                    }
                });
                if (avcontract.SubjectToLeaseAccounting)
                {
                    IEnumerable<VMActionAVReviewModel> actionedReviews = avcontract.Reviews.Where(r => r.ActionedReview != null).Select(a => a.ActionedReview);

                    //mae sure that assets are not being re-added
                    foreach (ContractAssetScheduleItemEditModel assetscheduleItem in avcontract.AssetSchedule)
                    {
                        if (assetscheduleItem.ValidTo.HasValue)
                        {
                            IEnumerable<VMActionAVReviewModel> relevantreviews = actionedReviews.Where(a => a.EffectiveDate > assetscheduleItem.ValidTo.Value);
                            IEnumerable<VMActionAVReviewModel> readdedAssets = relevantreviews.Where(rev => rev.AllCosts().Any(a => a.AssetID == assetscheduleItem.AssetID));
                            if (readdedAssets.Count() > 0)
                            {
                                ModelState.AddModelError("", @"Assets that are removed cannot be re-added.");
                            }
                        }
                    }

                }
                int minTemplateID = avcontract.Reviews.Where(r => r.ActionedReview != null).SelectMany(r => r.ActionedReview.Templates.Union(r.ActionedReview.UnchangedTemplates)).Select(t => t.InvoiceTemplateID).DefaultIfEmpty(0).Min();
                if (minTemplateID >= 0)
                {
                    minTemplateID = -1;
                }
                //Invoice Templates that are not attached to review costs
                avcontract.Templates.Where(t => t.InvoiceTemplateID < 1).ToList().ForEach(t =>
                {
                    t.InvoiceTemplateID = --minTemplateID;
                    t.Costs.ForEach(c =>
                    {
                        c.TemplateCostId = c.TemplateCostId > 0 ? c.TemplateCostId : --tcid;
                        c.InvoiceTemplateID = t.InvoiceTemplateID;
                    });
                });
                AgreedValueContractEditModel contract = SimpleMapper.MapNew<VMAgreedValueContractEditModel, AgreedValueContractEditModel>(avcontract);
                contract.Invoices.Clear();
                VMAgreedValueReviewEditModel lastActioned = null;
                Dictionary<int, int> templateDictionary = new Dictionary<int, int>();
                Dictionary<int, CostCategoryListModel> ccList = costCategoryService.GetAllCostCategories().ToDictionary(cc => cc.CostCategoryID);

                DateTime? TermEnd = avcontract.Terms.OrderBy(t => t.TermStart).Last(t => !t.IsOption || t.State == "Exercised").TermEnd;
                avcontract.Reviews.Where(r => r.ActionedReview != null).OrderBy(r => r.ActionedReview.EffectiveDate).ThenBy(r => r.ActionedReview.Priority).ToList().ForEach(r =>
                {
                    AgreedValueReviewEditModel newReview = contract.Reviews.First(r2 => r2.ReviewID == r.ReviewID);
                    if (r.ReviewType == "Adjustment")
                    {
                        // we need to set the review costs to the costs that have been
                        // actioned/changed by the adjustment, so that we can perform proper
                        // mapping next time
                        newReview.Costs = r.ActionedReview.Templates.SelectMany(t => t.ActionedCosts).Union(r.ActionedReview.ActionedCosts_NotInvoiced).Select(c => new AgreedValueReviewCostEditModel
                        {
                            AssetID = c.AssetID,
                            CategoryID = c.CategoryID,
                            Label = c.Label
                        }).ToList();
                    }
                    List<VMAgreedValueContractCostEditModel> reviewCosts = r.ActionedReview.ActionedCosts_NotInvoiced.ToList();
                    reviewCosts.AddRange(r.ActionedReview.UnactionedCosts_NotInvoiced);
                    reviewCosts.AddRange(r.ActionedReview.UnchangedTemplates.SelectMany(t => t.UnchangedCosts));
                    reviewCosts.AddRange(r.ActionedReview.UnchangedCosts);
                    r.ActionedReview.UnchangedTemplates.ForEach(t =>
                    {
                        if (t.InvoiceTemplateID > 0)
                        {
                            if (contract.Templates.Any(t2 => t2.InvoiceTemplateID == t.InvoiceTemplateID))
                            {
                                return;
                            }

                            contract.Templates.Add(invoiceService.GetInvoiceTemplate(t.InvoiceTemplateID));
                        }
                        else
                        {
                            t.InvoiceTemplateID = templateDictionary[t.InvoiceTemplateID];
                            InvoiceTemplateEditModel template = contract.Templates.First(t2 => t2.InvoiceTemplateID == t.InvoiceTemplateID);
                            t.ActionedCosts.Union(t.UnchangedCosts).ToList().ForEach(c =>
                            {
                                c.TemplateCostID = template.Costs.First(c2 => c2.AssetID == c.AssetID && c2.CategoryID == c.CategoryID && c2.Description == c.Label).TemplateCostId;
                            });
                        }
                    });
                    r.ActionedReview.Templates.ForEach(t =>
                    {
                        InvoiceTemplateEditModel template = contract.Templates.FirstOrDefault(t2 => t2.InvoiceTemplateID == t.InvoiceTemplateID);
                        if (template != null)
                        {
                            contract.Templates.Remove(template);
                        }
                        if (t.InvoiceTemplateID < 1)
                        {
                            templateDictionary.Add(t.InvoiceTemplateID, --minTemplateID);
                            t.InvoiceTemplateID = templateDictionary[t.InvoiceTemplateID];
                        }

                        template = new InvoiceTemplateEditModel
                        {
                            ContractID = contract.ContractID,
                            ReviewID = r.ReviewID,
                            Costs = t.ActionedCosts.Union(t.UnchangedCosts)
                                .Select(c => c.CreateTemplateCost(t.FirstInvoiceDate, TermEnd, t.InvoiceTemplateID)).ToList(),
                            CurrencyID = contract.CurrencyID,
                            Description = t.Description,
                            EndDate = null,
                            FirstInvoiceDate = t.FirstInvoiceDate,
                            Frequency = t.Frequency,
                            Group = t.InvoiceGroup,
                            InvoiceTemplateID = t.InvoiceTemplateID,
                            InvoiceTypeID = t.InvoiceTypeID,
                            IsPrepay = !t.ActionedCosts.Union(t.UnchangedCosts).All(a => a.PaidInArrears),
                            IsReceivable = contract.IsReceivable,
                            Modified = true,
                            New = true,
                            Notes = "",
                            Pattern = t.Pattern,
                            PurchaseOrderNo = "",
                            StartDate = r.ActionedReview.EffectiveDate.AddSeconds(r.ActionedReview.Priority),
                            VendorID = t.TemplateVendorID,
                            VendorName = t.TemplateVendorName
                        };
                        contract.Templates.Add(template);
                        reviewCosts.AddRange(t.ActionedCosts.Union(t.UnchangedCosts));
                    });
                    newReview.ActionedReview.Costs = reviewCosts.Select(c => SimpleMapper.MapNew<VMAgreedValueContractCostEditModel, AgreedValueContractCostEditModel>(c)).ToList();
                    newReview.ActionedReview.Costs.ForEach(ac =>
                    {
                        ac.CategoryIsLeaseAccountingSignificant = ccList[ac.CategoryID].LeaseAccountingSignificant;
                    });
                    if (lastActioned != null)
                    {
                        List<int> lastTemplates = lastActioned.ActionedReview.Templates.Union(lastActioned.ActionedReview.UnchangedTemplates).Select(t => t.InvoiceTemplateID).ToList();
                        contract.Templates.Where(t => lastTemplates.Contains(t.InvoiceTemplateID))
                            .Where(t => r.ActionedReview.UnchangedTemplates.All(t2 => t2.InvoiceTemplateID != t.InvoiceTemplateID))
                            .ToList()
                            .ForEach(t =>
                            {
                                t.EndDate = r.ActionedReview.EffectiveDate.AddDays(-1);
                            });
                    }
                    lastActioned = r;
                });
                if (lastActioned != null)
                {
                    List<int> lastTemplates = lastActioned.ActionedReview.Templates.Union(lastActioned.ActionedReview.UnchangedTemplates).Select(t => t.InvoiceTemplateID).ToList();
                    contract.Templates.Where(t => lastTemplates.Contains(t.InvoiceTemplateID))
                        .ToList()
                        .ForEach(t =>
                        {
                            t.EndDate = contract.Terms.Where(t2 => !t2.IsOption || t2.State == "Exercised").OrderBy(t2 => t2.TermStart).Last().TermEnd;
                        });
                }

                contract.Templates.Sort((t1, t2) => t1.StartDate.CompareTo(t2.StartDate));
                DateTime? contractEnd = contract.Terms.Where(t2 => !t2.IsOption || t2.State == "Exercised").OrderBy(t2 => t2.TermStart).Last().TermEnd;
                contract.Templates.ForEach(t =>
                {
                    t.Modified = true;
                    if (t.EndDate == null)
                    {
                        t.EndDate = contractEnd;
                    }
                });

                List<int> contextChildAssets = assetService.FindMatchingAssets("", null, status: assetService.GetAssetStatuses().ToArray()).Where(a => a.ParentID == ContextAssetID).Select(a => a.AssetID).ToList();
                if (isSubContract)
                {
                    contextChildAssets.Add((avcontract as VMSubContractEditModel).ParentContracts[0].SubContractMappings[0].AssetID);
                }
                contextChildAssets.Add(ContextAssetID);
                bool includedInCosts = contract.Reviews.Any(r => r.ActionedReview != null && r.ActionedReview.Costs.Any(c => contextChildAssets.Contains(c.AssetID)));
                if (!includedInCosts)
                {
                    ModelState.AddModelError("", @"The actioned review costs defined over the contract structure do not include any costs for the active asset, child assets, or subcontract assets. Saving the contract in this state would effectively hide the contract permanently and is prevented by the system.");
                }
                else
                {
                    if (!LeaseAccountingProviderFactory.Current.IsContractLeaseAccountingEnabled(contract, null, true, null))
                    {
                        avcontract.LeaseAccounting_AssetCategoryType = avcontract.LeaseAccounting_AssetCategoryType ?? "";
                        avcontract.LeaseAccounting_AccountingCode = avcontract.LeaseAccounting_AccountingCode ?? "";
                        avcontract.LeaseAccounting_LedgerSystem = avcontract.LeaseAccounting_LedgerSystem ?? "";
                        avcontract.LeaseAccounting_LeaseType = avcontract.LeaseAccounting_LeaseType ?? "";
                    }
                    if (ModelState.IsValid)
                    {
                        TemplateUpdateResult result = new TemplateUpdateResult();
                        if (TrySave(() =>
                        {
                            using (RepositoryTransactionScope scope = new RepositoryTransactionScope())
                            {
                                // Add any child assets we need to make this work
                                SaveNewChildAssetsForSubContract(contract, avcontract as VMSubContractEditModel);
                                //SaveAssetScheduleItemDetails(avcontract);
                                if (avcontract.ContractID < 0)
                                {
                                    contractService.CreateContract(contract, journal);
                                }
                                else
                                {
                                    result = contractService.UpdateContract(contract, journal);
                                }
                                scope.Complete();
                            }
                        }))
                        {
                            //only return a result if there are no ee\xceptions
                            return result;
                        }
                    }
                }
            }
            else
            {
                SystemContext.AuditLog.AddAuditEntry("Contract", "SaveAVContract", "Failed", $"Validation Failed for AV Contract {avcontract.ContractID} - {avcontract.Description}");
            }
            return null;
        }

        /// <summary>
        /// The SaveExitCost.
        /// </summary>
        /// <param name="model">The model<see cref="ExitCostEditModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult SaveExitCost(ExitCostEditModel model)
        {
            ViewBag.IncludeActionsForRows = true;
            if (ModelState.IsValid)
            {
                return ExtendedJson(new
                {
                    success = true,
                    row = RenderVariantPartialViewToString("DisplayTemplates/ExitCostEditModel", model)
                });
            }
            return PartialView("EditorTemplates/ExitCostEditModel", model);
        }

        /// <summary>
        /// The SaveGuarantee.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="model">The model<see cref="GuaranteeEditModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult SaveGuarantee(int ID, GuaranteeEditModel model)
        {
            if (ModelState.IsValid)
            {
                model.Guarantors = model.Guarantors.Distinct(new GuarantorEqualityComparer<GuarantorEditModel>()).ToList();
                GuaranteeViewModel vm = new GuaranteeViewModel();
                SimpleMapper.Map(model, vm);
                model.Guarantors.ForEach(g => g.Guarantor = contactService.GetContactDisplayName(g.GuarantorID));
                return ExtendedJson(new { success = true, row = RenderVariantPartialViewToString("DisplayTemplates/GuaranteeViewModel", vm) });
            }
            ViewBag.GuaranteeTypes = contractService.GetGuaranteeTypes();
            return PartialView("EditorTemplates/GuaranteeEditModel", model);
        }

        /// <summary>
        /// The SaveIncentive.
        /// </summary>
        /// <param name="model">The model<see cref="IncentiveEditModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult SaveIncentive(IncentiveEditModel model)
        {
            ViewBag.IncludeActionsForRows = true;
            if (ModelState.IsValid)
            {
                return ExtendedJson(new
                {
                    success = true,
                    row = RenderVariantPartialViewToString("DisplayTemplates/IncentiveEditModel", model)
                });
            }
            return PartialView("EditorTemplates/IncentiveEditModel", model);
        }

        /// <summary>
        /// The SaveInitialCost.
        /// </summary>
        /// <param name="model">The model<see cref="InitialCostEditModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult SaveInitialCost(InitialCostEditModel model)
        {
            ViewBag.IncludeActionsForRows = true;
            if (ModelState.IsValid)
            {
                return ExtendedJson(new
                {
                    success = true,
                    row = RenderVariantPartialViewToString("DisplayTemplates/InitialCostEditModel", model)
                });
            }
            return PartialView("EditorTemplates/InitialCostEditModel", model);
        }
        /// <summary>
        /// The SaveLeaseAccountingReview.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="LeaseAccountingReviewID">The LeaseAccountingReviewID<see cref="int"/>.</param>
        /// <param name="mode">The mode<see cref="string"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult SaveLeaseAccountingReview(int ID, int LeaseAccountingReviewID, string mode)
        {
            if (!UserContext.Current.EvaluateAccess(true,
                TestAssetIsAccessible,
                LeaseAccountingReviewPermissions.Create))
            {
                return JsonUnauthorized();
            }

            ISystemRepository sysRepo = ServiceLocator.Current.GetInstance<ISystemRepository>();
            Guid changeSet = Guid.NewGuid();
            if (mode != "delete")
            {
                if (!(contractService.GetContractEdit(ID, false) is AgreedValueContractEditModel contract))
                {
                    return ExtendedJson(new
                    {
                        success = false,
                        message =
                            "The contract underlying this review no longer exists and may have been removed by another user"
                    });
                }

                CurrencyViewModel currency = localeService.GetCurrency(contract.CurrencyID);
                LeaseAccountingReviewEditModel review = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, true, true);
                LeaseAccountingReviewEditModel previousReview = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewForContract(contract.ContractID, true, true);
                switch (review.LastWizardPage)
                {
                    case 1: // save review notes
                        foreach (LeaseAccountingReviewReviewEditModel r in review.Reviews)
                        {
                            AgreedValueReviewEditModel r2 = contract.Reviews.First(r3 => r3.ReviewID == r.ReviewID);
                            string note = string.IsNullOrWhiteSpace(Request.Params[r.ReviewID + "_reviewNotes"]) ? "" : Request.Params[r.ReviewID + "_reviewNotes"];
                            if (note == r.Note)
                            {
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(note))
                            {
                                sysRepo.AddAuditEntry(review.EntityID, "LeaseAccounting Review", "LeaseAccounting Review", changeSet, 0, ChangeTypes.Delete,
                                    r2.ReviewType + " Review - " + r2.ReviewDate.ToString(UserContext.Current.DateFormat),
                                    r.Note,
                                    "-"
                                );
                            }
                            else if (string.IsNullOrWhiteSpace(r.Note))
                            {
                                r.Note = note;
                                sysRepo.AddAuditEntry(review.EntityID, "LeaseAccounting Review", "LeaseAccounting Review", changeSet, 0, ChangeTypes.Add,
                                    r2.ReviewType + " Review - " + r2.ReviewDate.ToString(UserContext.Current.DateFormat),
                                    "-",
                                    r.Note
                                );
                            }
                            else
                            {
                                sysRepo.AddAuditEntry(review.EntityID, "LeaseAccounting Review", "LeaseAccounting Review", changeSet, 0, ChangeTypes.Update,
                                    r2.ReviewType + " Review - " + r2.ReviewDate.ToString(UserContext.Current.DateFormat),
                                    r.Note,
                                    note
                                );
                                r.Note = note;
                            }
                        }
                        break;

                    case 2:// save option assumptions, notes, files, early termination fields                      
                        Tuple<bool, string> result = SaveLeaseAccountingReviewDurationTab(review, sysRepo, changeSet, currency);
                        if (!result.Item1)
                        {
                            return ExtendedJson(new
                            {
                                success = false,
                                message = result.Item2
                            }, JsonRequestBehavior.AllowGet);
                        }
                        break;

                    case 3: // save sighted/sighted rate
                        bool sighted;
                        string value = Request.Params["rateSighted"];
                        if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out sighted))
                        {
                            review.LeaseAccounting_DiscountRateSighted = sighted;
                            //if(review.)
                            //commented out does this need to happen since we're always getting the correct rate via the draft? -Peter
                            //review.IFRSDiscountRate = decimal.Parse(Request.Params["latestDiscountRate"]);
                            review.DraftProjectedDuration = review.ProjectWholeYears();
                        }
                        break;

                    case 4:
                        break;
                }
                switch (mode)
                {
                    case "goto":
                        int newPage;
                        int.TryParse(Request.Params["WizardPage"], out newPage);
                        review.LastWizardPage = int.TryParse(Request.Params["WizardPage"], out newPage) ? newPage : review.LastWizardPage;
                        break;

                    case "next":
                        if (review.LastWizardPage == 3 && LoisProvider.IsEnabled)
                        {
                            DateTime projectedEnd = review.ProjectTermEnd();

                            int years = review.LeaseAccounting_StartDate.YearsBetween(projectedEnd, true);
                            switch (LoisProvider.GetDiscountRateForDuration(years, out decimal rate))
                            {
                                case Domain.Services.LeaseAccounting.Providers.Lois.DiscountRateError.NotFound:
                                    return ExtendedJson(new
                                    {
                                        success = false,
                                        message = "Unable to save " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review progress - no discount rate is available for a duration of " + years + " years"
                                    });

                                case Domain.Services.LeaseAccounting.Providers.Lois.DiscountRateError.Expired:
                                    return ExtendedJson(new
                                    {
                                        success = false,
                                        message = "Unable to save " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review progress - the latest discount rate has expired",
                                        reload = true
                                    });
                            }
                            //we only care about the discount rate if the end has changed since the last export
                            if (previousReview != null && projectedEnd != previousReview.ProjectTermEnd() && review.LeaseAccounting_DiscountRate != rate)
                            {
                                return ExtendedJson(new
                                {
                                    success = false,
                                    message = "Unable to save " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review progress - the latest discount rate has changed",
                                    reload = true
                                });
                            }
                        }
                        if (review.LastWizardPage == 1)
                        {
                            if (DateTime.TryParseExact(Request.Params["LeaseAccountingStartDate"], UserContext.Current.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out DateTime leaseAccountingStartDate))
                            {
                                review.LeaseAccounting_StartDate = leaseAccountingStartDate;                          
                            };
                        }
                        review.LastWizardPage += 1;
                        break;

                    case "prev":
                        review.LastWizardPage -= 1;
                        break;

                    case "reset":
                        review.LastWizardPage = 1;
                        break;

                    case "submit":
                        // perform final validation
                        List<ValidationResult> errors = new List<ValidationResult>();
                        try
                        {
                            errors.AddRange(LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(review, contract, new ValidationContext(review)));
                            leaseAccountingService.SetLeaseAccountingReviewState(review, "Submitted", LeaseAccountingReview_ProcessCode.IMPORT_FOR_LEASE_ACCOUNTING);
                            if (LeaseAccountingOptions.Get<bool>(LeaseAccountingOptions.LeaseAccountingReviewSimplification))
                            {
                                GenerateNormalizationSummary(review.LeaseAccountingReviewID);
                            }

                        }
                        catch (Exception ex)
                        {
                            EventLogHelper.LogException("Failed to validate and submit lease accounting review", ex);
                            errors.Add(new ValidationResult(ex.Message));
                        }
                        if (errors.Count > 0)
                        {
                            return ExtendedJson(new
                            {
                                success = false,
                                errors = errors.Select(e => e.ErrorMessage).Distinct()
                            });
                        }
                        string note = Request.Params["submitNote"];
                        if (!string.IsNullOrWhiteSpace(note))
                        {
                            sysRepo.AddJournalEntry(review.EntityID, LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " Review", LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingLabel) + " Review", note);
                        }

                        break;
                }
                if (review.LastWizardPage == 3 && !LeaseAccountingOptions.Get<bool>(LeaseAccountingOptions.EnableDiscountRates))
                {
                    review.LastWizardPage = 4;
                }
                if (!TrySave(() => leaseAccountingService.UpdateLeaseAccountingReview(review)))
                {
                    return ExtendedJson(new
                    {
                        success = false,
                        message =
                            "An error occurred updating the " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review progress. This review may have been removed by another user."
                    }, JsonRequestBehavior.DenyGet);
                }

                foreach (Guid t in review.Terms.Where(t => t.FileID != null).Select(t => t.FileID.Value).ToList())
                {
                    fileService.FinalizeFileByID(t);
                }
                int contextAsset = review.PrimaryAsset.AssetID;
                return ExtendedJson(new
                {
                    success = true,
                    message = "Successfully updated " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review progress",
                    SuccessRedirectUrl = Url.Action("Detail", "Asset", new { ID = contextAsset, Tab = "contracts", section = "view", contractid = ID, contractTab = "view" })
                }, JsonRequestBehavior.DenyGet);
            }
            if (TrySave(() => leaseAccountingService.DeleteDraftLeaseAccountingReviewForContract(ID)))
            {
                return ExtendedJson(new
                {
                    success = true,
                    message = "Successfully deleted draft " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review"
                }, JsonRequestBehavior.DenyGet);
            }

            return ExtendedJson(new
            {
                success = false,
                message = "An error occurred deleting the " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review progress. This review may have been removed by another user."
            }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// The SaveMakeGoodCost.
        /// </summary>
        /// <param name="model">The model<see cref="MakeGoodCostEditModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult SaveMakeGoodCost(MakeGoodCostEditModel model)
        {
            ViewBag.IncludeActionsForRows = true;
            if (ModelState.IsValid)
            {
                return ExtendedJson(new
                {
                    success = true,
                    row = RenderVariantPartialViewToString("DisplayTemplates/MakeGoodCostEditModel", model)
                });
            }
            return PartialView("EditorTemplates/MakeGoodCostEditModel", model);
        }

        /// <summary>
        /// The SaveNewChildAssetsForSubContract.
        /// </summary>
        /// <param name="contract">The contract<see cref="AgreedValueContractEditModel"/>.</param>
        /// <param name="subcontract">The subcontract<see cref="VMSubContractEditModel"/>.</param>
        public void SaveNewChildAssetsForSubContract(AgreedValueContractEditModel contract, VMSubContractEditModel subcontract)
        {
            if (subcontract == null)
            {
                return;
            }

            Dictionary<int, AssetEditModel> newChildList = new Dictionary<int, AssetEditModel>();
            contract.ParentContracts = subcontract.ParentContracts.SelectMany(pc => pc.SubContractMappings)
            .Where(sc => sc.SubcontractedEnabled)
            .Select(sc =>
            {
                SubContractMappingEditModel map = SimpleMapper.Map<VMSubContractMappingModel, SubContractMappingEditModel>(sc);
                if (sc.SubContractOptions == VMSubContractMappingModel.SubContractAssetOptions.CreateNewAsset)
                {
                    AssetViewModel parentAsset = assetService.GetAssetView(sc.ParentAssetID, false, false, false, false, false);

                    AssetEditModel childAsset = new AssetEditModel
                    {
                        AssetID = sc.ChildAssetDetails.ID,
                        ParentID = sc.ParentAssetID,
                        Ownership = string.IsNullOrEmpty(sc.ChildAssetDetails.Ownership) ? "OTHER" : sc.ChildAssetDetails.Ownership,
                        Name = sc.ChildAssetDetails.Name,
                        BusinessUnit = string.IsNullOrEmpty(sc.ChildAssetDetails.BusinessUnit) ? parentAsset.BusinessUnit : sc.ChildAssetDetails.BusinessUnit,
                        LegalEntity = string.IsNullOrEmpty(sc.ChildAssetDetails.LegalEntity) ? parentAsset.LegalEntity : sc.ChildAssetDetails.LegalEntity,
                        AssetTypeID = sc.ChildAssetDetails.AssetType ?? parentAsset.AssetTypeID,
                        Status = string.IsNullOrEmpty(sc.ChildAssetDetails.Status) ? parentAsset.Status : sc.ChildAssetDetails.Status,
                        //have it automatically inherit the parent's jurisidiction code
                        DefaultJurisdictionCode = parentAsset.DefaultJurisdictionCode
                    };
                    assetService.AddAsset(childAsset);
                    newChildList.Add(sc.ChildAssetDetails.ID, childAsset);

                    // Set new Asset IDs for our temporary Assets for contract costs
                    contract.Reviews.ForEach(r =>
                    {
                        r.Costs.Where(c => c.AssetID == sc.ChildAssetDetails.ID).ToList().ForEach(c =>
                        {
                            c.AssetID = childAsset.AssetID;
                        });
                        if (r.ActionedReview != null)
                        {
                            r.ActionedReview.Costs.Where(c => c.AssetID == sc.ChildAssetDetails.ID).ToList().ForEach(c =>
                            {
                                c.AssetID = childAsset.AssetID;
                            });
                        }
                    });
                    map.AssetID = childAsset.AssetID;
                }
                return map;
            }).ToList();
            contract.Templates.ForEach(t => t.Costs.ForEach(c => c.AssetID = newChildList.ContainsKey(c.AssetID) ? newChildList[c.AssetID].AssetID : c.AssetID));
        }

        /// <summary>
        /// The SaveNewSubContractMapping.
        /// </summary>
        /// <param name="model">The model<see cref="AddNewContractMappingViewModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult SaveNewSubContractMapping(AddNewContractMappingViewModel model)
        {
            try
            {
                AgreedValueContractViewModel subcontract = contractService.GetContractView(model.Mapping.ContractID) as AgreedValueContractViewModel;

                SubContractMappingEditModel editModel = new SubContractMappingEditModel
                {
                    ContractID = model.Mapping.ContractID,
                    ParentContractID = model.Mapping.ParentContractID,
                    Percentage = 0
                };
                if (model.Mapping.SubContractOptions == VMSubContractMappingModel.SubContractAssetOptions.UseParent)
                {
                    editModel.AssetID = model.Mapping.ParentAssetID;
                }
                else if (model.Mapping.SubContractOptions == VMSubContractMappingModel.SubContractAssetOptions.UseExistingChild)
                {
                    editModel.AssetID = model.Mapping.AssetID;
                }
                else
                {
                    AssetViewModel parentAsset = assetService.GetAssetView(model.Mapping.ParentAssetID, false, false, false, false, false);
                    AssetEditModel childAsset = new AssetEditModel
                    {
                        ParentID = model.Mapping.ParentAssetID,
                        Ownership = string.IsNullOrEmpty(model.ChildAsset.Ownership) ? "OTHER" : model.ChildAsset.Ownership,
                        Name = model.ChildAsset.Name,
                        BusinessUnit = string.IsNullOrEmpty(model.ChildAsset.BusinessUnit) ? parentAsset.BusinessUnit : model.ChildAsset.BusinessUnit,
                        LegalEntity = string.IsNullOrEmpty(model.ChildAsset.LegalEntity) ? parentAsset.LegalEntity : model.ChildAsset.LegalEntity,
                        AssetTypeID = model.ChildAsset.AssetType ?? parentAsset.AssetTypeID,
                        Status = string.IsNullOrEmpty(model.ChildAsset.Status) ? parentAsset.Status : model.ChildAsset.Status
                    };
                    assetService.AddAsset(childAsset);
                    editModel.AssetID = childAsset.AssetID;
                }
                contractService.CreateSubContractMapping(editModel);
                return Json(new { success = true });
            }
            catch (DomainValidationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Save a new or edited RB review.
        /// </summary>
        /// <param name="review"> the updated review details.</param>
        /// <param name="terms">  list of terms currently on the contract.</param>
        /// <param name="reviews">list of reviews currently on the contract.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult SaveRBReview(VMRateReviewEditModel review, List<TermEditModel> terms, List<VMRateReviewEditModel> reviews)
        {
            ModelState.Clear();
            terms = (terms ?? new List<TermEditModel>()).OrderBy(t => t.TermStart).ToList();

            if (terms.Count < 1)
            {
                return ExtendedJson(new { success = false, message = "An initial term must be added to the contract before costs and reviews can be defined" });
            }

            ViewBag.StartDate = terms[0].TermStart;
            DateTime first = terms[0].TermStart;
            if (review.ReviewDate.Date <= first.Date)
            {
                ModelState.AddModelError("ReviewDate", @"The review must be after the commencement date of the contract");
            }

            TryValidateModel(review);

            if (!ModelState.IsValid)
            {
                return ExtendedJson(new
                {
                    success = false,
                    html = RenderVariantPartialViewToString("EditorTemplates/RateReviewEditModel", review)
                });
            }

            review.IsNew = false;
            List<string> rows = new List<string> { RenderVariantPartialViewToString("DisplayTemplates/RateReviewEditModel", review) };
            if (!review.Recurring)
            {
                return ExtendedJson(new
                {
                    success = true,
                    rows
                });
            }

            for (int i = 0; i < review.Instances; i++)
            {
                review.Guid = Guid.NewGuid().ToString();
                switch (review.Pattern)
                {
                    case "Months":
                        review.ReviewDate = review.ReviewDate.AddMonths(review.Interval);
                        break;

                    case "Years":
                        review.ReviewDate = review.ReviewDate.AddYears(review.Interval);
                        break;
                }
                rows.Add(RenderVariantPartialViewToString("DisplayTemplates/RateReviewEditModel", review));
            }

            return ExtendedJson(new
            {
                success = true,
                rows
            });
        }

        /// <summary>
        /// The SearchContracts.
        /// </summary>
        /// <param name="parameters">The parameters<see cref="ContractSearchParameters"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult SearchContracts(ContractSearchParameters parameters)
        {
            List<SearchContractResult> results = new List<SearchContractResult>();
            contractService.SearchContracts(parameters).GroupBy(c => c.ContractType).ToList().ForEach(g =>
            {
                results.Add(new SearchContractResult { Type = "category", Label = g.Key, Assets = Array.Empty<string>() });
                g.OrderBy(g2 => g2.Assets().ToList().OrderBy(a => a).First()).ThenBy(c => c.Description).ToList().ForEach(c =>
                {
                    SearchContractResult result = new SearchContractResult { Type = "contract", VendorId = c.VendorID, VendorName = c.Vendor, Label = c.Description, Id = c.ContractID, Assets = c.Assets().OrderBy(a => a).ToArray() };
                    results.Add(result);
                });
            });
            return ExtendedJson(new { success = true, rows = results }, JsonRequestBehavior.DenyGet);
        }

        /// <summary>
        /// Present a dialog.
        /// </summary>
        /// <param name="currencyID"></param>
        /// <param name="reviewType"></param>
        /// <param name="costs">  </param>
        /// <param name="reviews"></param>
        /// <param name="reviewDate"></param>
        /// <param name="guid"></param>
        /// <param name="ParentContracts">The ParentContracts<see cref="List{VMParentContractsModel}"/></param>
        /// <param name="AssetSchedule">The AssetSchedule<see cref="List{ContractAssetScheduleItemEditModel}"/></param>
        /// <returns></returns>
        public ExtendedJsonResult SelectAVCostsToReview(int currencyID, string reviewType, string costs, List<VMAgreedValueReviewEditModel> reviews, DateTime reviewDate, string guid, List<VMParentContractsModel> ParentContracts, List<ContractAssetScheduleItemEditModel> AssetSchedule)
        {
            // unwrap the costs that are currently selected
            List<AgreedValueReviewCostEditModel> currentCosts = JsonConvert.DeserializeObject<List<AgreedValueReviewCostEditModel>>(Request.Params["costs"] ?? "[]", new LocalizedDateTimeJsonConverter()).Where(c => c.AssetID > 0).ToList();
            reviews.Sort((a, b) =>
            {
                if (a.ActionedReview == null)
                {
                    if (b.ActionedReview == null)
                    {
                        return a.ReviewDate.CompareTo(b.ReviewDate);
                    }
                    return a.ReviewDate.AddSeconds(1).CompareTo(b.ActionedReview.EffectiveDate);
                }
                if (b.ActionedReview == null)
                {
                    return a.ActionedReview.EffectiveDate.CompareTo(b.ReviewDate.AddSeconds(1));
                }
                return a.ActionedReview.EffectiveDate.Date.AddSeconds(a.ActionedReview.Priority).CompareTo(b.ActionedReview.EffectiveDate.Date.AddSeconds(b.ActionedReview.Priority));
            });
            VMAgreedValueReviewEditModel currentReview = reviews.Find(r => r.Guid == guid);

            VMAgreedValueReviewEditModel lastActionedReview = reviews
                .Where(r => r.ActionedReview != null)
                .LastOrDefault(r => r.ActionedReview.EffectiveDate < reviewDate || (r.ActionedReview.EffectiveDate == reviewDate && (currentReview == null || currentReview.ActionedReview == null || r.ActionedReview.Priority < currentReview.ActionedReview.Priority)));

            if (lastActionedReview == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "Cannot find costs to add to review. Ensure commencing costs have been specified for the contract"
                });
            }

            Dictionary<int, string> assets = assetService.GetAssetSelectList(currencyID).ToDictionary(c => int.Parse(c.Key), c => c.Name);
            if (ParentContracts != null)
            {
                ParentContracts.SelectMany(pc => pc.SubContractMappings.Select(c => c.ChildAssetDetails)).ToList().ForEach(a =>
                {
                    if (!assets.ContainsKey(a.ID))
                    {
                        assets.Add(a.ID, a.Name);
                    }
                });
            }
            if (AssetSchedule != null)
            {
                AssetSchedule.ForEach(a =>
                {
                    if (!assets.ContainsKey(a.AssetID))
                    {
                        assets.Add(a.AssetID, a.Asset);
                    }
                });
            }
            ViewBag.Assets = assets;
            ViewBag.Categories = costCategoryService.GetAllCostCategories().ToDictionary(c => c.CostCategoryID, c => c.DisplayName());
            List<VMSelectableCostEditModel> scosts = new List<VMSelectableCostEditModel>();
            List<VMAgreedValueContractCostEditModel> allCosts = lastActionedReview.ActionedReview.Templates.SelectMany(t => t.ActionedCosts.Union(t.UnchangedCosts)).ToList();
            allCosts.AddRange(lastActionedReview.ActionedReview.UnchangedTemplates.SelectMany(t => t.ActionedCosts.Union(t.UnchangedCosts)).ToList());
            allCosts.AddRange(lastActionedReview.ActionedReview.UnactionedCosts_NotInvoiced);
            allCosts.AddRange(lastActionedReview.ActionedReview.ActionedCosts_NotInvoiced);
            allCosts.AddRange(lastActionedReview.ActionedReview.UnchangedCosts);
            switch (reviewType)
            {
                case "Adjustment":
                case "Fixed":
                case "Fixed%":
                case "Market":
                case "CPI":
                    ViewBag.ShowLabelColumn = true;
                    scosts = allCosts.Select(c =>
                    new VMSelectableCostEditModel
                    {
                        AssetID = c.AssetID,
                        CategoryID = c.CategoryID,
                        Label = c.Label,
                        Selected = currentCosts.Any(c2 => c2.CategoryID == c.CategoryID && c2.AssetID == c.AssetID && (c2.Label ?? "") == (c.Label ?? "")),
                        Json = JsonConvert.SerializeObject(c)
                    }).ToList();
                    break;
            }
            return ExtendedJson(new
            {
                success = true,
                html = RenderVariantPartialViewToString("Dialog/SelectCosts", scosts)
            });
        }

        /// <summary>
        /// The SelectDocument.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult SelectDocument(int id)
        {
            ViewBag.ContractID = id;
            return PartialView("Dialog/SelectDocument");
        }

        /// <summary>
        /// The SelectSingleContractDialog.
        /// </summary>
        /// <param name="IsReceivable">The IsReceivable<see cref="bool"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        [HttpGet]
        public PartialViewResult SelectSingleContractDialog(bool IsReceivable)
        {
            return PartialView("Dialog/SelectSingleContract", new ContractSearchParams
            {
                ContractTypes = contractTypeService.GetContractTypes()
                .Where(t => t.Direction == 'B' || t.Direction == 'R' == IsReceivable)
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Name).ToList()),
                IsReceivable = IsReceivable
            });
        }

        /// <summary>
        /// The SelectTemplate.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult SelectTemplate(int id)
        {
            ViewBag.ContractID = id;
            return PartialView("Dialog/SelectDocumentTemplate", documentService.GetDocumentTemplates(id).ToList());
        }

        /// <summary>
        /// The SetLeaseAccountingInclusion.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="include">The include<see cref="bool"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult SetLeaseAccountingInclusion(int ID, bool include)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Base))
            {
                return ExtendedJson(new { message = "You are unauthorised to do this action. Please contact your administrator." });
            }

            if (!(contractService.GetContractEdit(ID, false) is AgreedValueContractEditModel editmodel))
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The specified contract does not exist or is not an agreed value contract"
                });
            }

            if (!editmodel.NeedsOverride)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "This contract is already enabled for " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingLabel) + " reporting"
                });
            }

            contractService.OverrideLeaseAccountingMaterialityTest(ID, include);
            return ExtendedJson(new
            {
                success = true,
                message = "Contract is now enabled for " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingLabel) + " reporting"
            });
        }

        /// <summary>
        /// Import the one time payments.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult ImportOneTimePayments(int ID)
        {
            AgreedValueContractEditModel contract = contractService.GetContractEdit(ID, false) as AgreedValueContractEditModel;
            LeaseAccountingReviewEditModel draftLeaseAccountingReview = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, true);
            draftLeaseAccountingReview.State = LeaseAccountingConstants.LeaseAccountingStates.Submitted;
            LeaseAccountingReviewEditModel draft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, true);
            //exercising an option requires a formal lease accounting review
            draft.IsFormalLeaseAccountingReview = true;
            leaseAccountingService.SetLeaseAccountingReviewState(draft, "Submitted", LeaseAccountingReview_ProcessCode.ONEOFF_PAYMENT);
            return ExtendedJson(new
            {
                success = true,
                message = "One Time Payments Imported"
            });
        }

        /// <summary>
        /// Rollback Contract.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult RollbackContract(int ID)
        {
            AgreedValueContractEditModel original = contractService.GetContractEdit(ID, false) as AgreedValueContractEditModel;
            try
            {
                AgreedValueContractEditModel contract = contractService.GetContractEdit(ID, true) as AgreedValueContractEditModel;
                //Best practice according to LeaseAccelerator Guide to error corrections and modifications
                contract.ReferenceNo += "-ROLLBACK";
                contractService.UpdateContract(contract, "Update Schedule number in accordance with best practices");
                contractService.ArchiveRollbackContract(ID);
                LeaseAccountingReviewEditModel draft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, true);
                leaseAccountingService.SetLeaseAccountingReviewState(draft, "Submitted", LeaseAccountingReview_ProcessCode.ROLLBACK);
                return ExtendedJson(new
                {
                    success = true,
                    message = "Contract rollback successfully"
                });
            }
            catch (Exception ex)
            {
                contractService.ReverseRollbackContract(ID);
                EventLogHelper.LogException($"RollbackContract failed", ex);
            }
            return ExtendedJson(new
            {
                success = false,
                message = "An error occurred while trying to rollback contract"
            });
        }


        /// <summary>
        /// The CloneIntoNewContractAction
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/></param>
        /// <returns>The <see cref="PartialViewResult"/></returns>
        public PartialViewResult CloneIntoNewContractAction(int ID)
        {
            AgreedValueContractEditModel contract = contractService.GetContractEdit(ID) as AgreedValueContractEditModel;
            if (contract == null)
            {
                return PartialView("Partial/Error", new { message = "The Contract you tried to clone could not be found or It's not Agreed Contract." });
            }
            CloneIntoNewContractEditModel model = new CloneIntoNewContractEditModel();
            var assets = new List<AssetEditModel>();
            Dictionary<int, string> dict = new Dictionary<int, string>();
            if (contract.AssetSchedule.Count > 0)
            {
                foreach (var assetSchedule in contract.AssetSchedule)
                {
                    var asset = assetService.GetAssetEdit(assetSchedule.AssetID);
                    dict.Add(asset.AssetID, asset.Name);
                    asset.Name = string.Empty;
                    asset.ReferenceNo = string.Empty;
                    asset.CustomFieldValues.ForEach(c => c.EntityID = Guid.Empty);
                    assets.Add(asset);
                }
            }
            ViewBag.AssetDict = dict;
            model.Assets = assets;

            return PartialView("EditorTemplates/CloneIntoNewContractEditModel", model);
        }

        /// <summary>
        /// Clone Into New Contract.
        /// </summary>
        /// <param name="ID">The ID.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        [HttpPost]
        public ActionResult CloneIntoNewContract(int ID, CloneIntoNewContractEditModel model = null)
        {
            try
            {
                var contractByReferece = contractService.GetContractEdit(model.ContractReferenceNo);
                return ExtendedJson(new
                {
                    success = false,
                    message = string.Format("Unable to Clone, A contract with same Schedule number was found."),
                });
            }
            catch
            { }
            foreach (var m in model.Assets)
            {
                try
                {
                    var asset = assetService.GetAssetEdit(m.ReferenceNo);
                    return ExtendedJson(new
                    {
                        success = false,
                        message = string.Format("Unable to Clone, An Asset with the same reference number was found."),
                    });
                }
                catch
                {
                    continue;
                }
            }

            ContractEditModel contract = contractService.GetContractEdit(ID);
            ContractEditModel Originalcontract = contractService.GetContractEdit(ID);

            Dictionary<Guid, IEnumerable<CustomFieldValue>> CustomFieldValuesEntityDict = new Dictionary<Guid, IEnumerable<CustomFieldValue>>();
            if (contract == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = string.Format("The Contract you tried to view could not be found."),
                });
            }
            int[] newAssetsIds = new int[model.Assets.Count];
            try
            {
                if (contract is AgreedValueContractEditModel)
                {
                    AgreedValueContractEditModel agreedValueContract = contract as AgreedValueContractEditModel;
                    //use a new entity id
                    contract.EntityID = Guid.NewGuid();
                    contract.CustomFieldValues.ForEach(r => r.EntityID = contract.EntityID);
                    contract.Lifecycle_state = "In-Abstraction";
                    contract.ClonedFromContractID = contract.ContractID;
                    contract.ContractID = -1;
                    contract.IsArchived = false;
                    string journal = "";

                    //update lease_start_date
                    agreedValueContract.LeaseAccounting_StartDate = model.LeaseAccounting_StartDate;
                    agreedValueContract.ReferenceNo = model.ContractReferenceNo;
                    var originalAssetSchedules = agreedValueContract.AssetSchedule;
                    var originalReviews = agreedValueContract.Reviews;
                    var originalTemplates = agreedValueContract.Templates;
                    var originalInvoices = agreedValueContract.Invoices;
                    Dictionary<int, int> AssetMatchingClone = new Dictionary<int, int>();

                    foreach (var asset in model.Assets)
                    {
                        var originalAssetSchedule = originalAssetSchedules.Where(a => a.AssetID == asset.AssetID).FirstOrDefault();
                        var originalAsset = assetService.GetAssetEdit(originalAssetSchedule.AssetID);
                        var originalassetid = originalAsset.AssetID;
                        var assetEnitity = Guid.NewGuid();
                        Guid oldAssetEnity = originalAsset.EntityID;
                        AssetTypeEditModel type = assetTypesService.GetAssetType(originalAsset.AssetTypeID, originalAsset.Ownership);
                        List<CustomFieldValueEditModel> values = extendableService.GetExtendableEntityValuesEdit(oldAssetEnity, type.EntityID, "AssetType", asset.Ownership);
                        var assetEntity = MappingContext.Instance.Map<List<CustomFieldValueEditModel>, IEnumerable<CustomFieldValue>>(values);
                        CustomFieldValuesEntityDict.Add(assetEnitity, assetEntity);


                        originalAsset.AssetID = 0;
                        originalAsset.Name = asset.Name;
                        originalAsset.ReferenceNo = asset.ReferenceNo;
                        var oldAddress = originalAsset.Address;
                        originalAsset.Address = oldAddress;
                        originalAsset.Address.AddressID = -1;
                        originalAsset.Address.LA_ID = null;
                        originalAsset.AddressID = -1;
                        originalAsset.EntityID = assetEnitity;
                        originalAsset.CustomFieldValues.ForEach(c => c.EntityID = originalAsset.EntityID);
                        assetService.AddAsset(originalAsset);

                        AssetMatchingClone.Add(asset.AssetID, originalAsset.AssetID);
                        int i = 0;
                        newAssetsIds[i] = originalAsset.AssetID;

                        //loop reviews.costs and reviews.actionedreview.costs
                        if (originalReviews.Count > 0)
                        {
                            originalReviews.ForEach(r =>
                            {
                                r.Costs.ForEach(c1 =>
                                {
                                    if (c1.AssetID == originalAssetSchedule.AssetID)
                                    {
                                        c1.AssetID = originalAsset.AssetID;
                                        c1.Asset = originalAsset.Name;
                                    }
                                });
                                if (r.ActionedReview != null)
                                {
                                    r.ActionedReview.Costs.ForEach(c2 =>
                                    {
                                        if (c2.AssetID == originalAssetSchedule.AssetID)
                                        {
                                            c2.AssetID = originalAsset.AssetID;
                                            c2.Asset = originalAsset.Name;
                                        }
                                    });
                                }
                            });
                        }
                        if (originalInvoices.Count > 0)
                        {
                            originalInvoices.ForEach(r1 =>
                            {
                                r1.Costs.ForEach(c3 =>
                                {
                                    if (c3.AssetID == originalAssetSchedule.AssetID)
                                    {
                                        c3.AssetID = originalAsset.AssetID;
                                        c3.AssetName = originalAsset.Name;
                                    }
                                });
                            });
                        }
                        if (originalTemplates.Count > 0)
                        {
                            originalTemplates.ForEach(r2 =>
                            {
                                r2.Costs.ForEach(c4 =>
                                {
                                    if (c4.AssetID == originalAssetSchedule.AssetID)
                                    {
                                        c4.AssetID = originalAsset.AssetID;
                                        c4.AssetName = originalAsset.Name;
                                    }
                                });
                            });
                        }

                        //update the original asset schedule
                        originalAssetSchedule.AssetID = originalAsset.AssetID;
                        originalAssetSchedule.Asset = originalAsset.Name;
                    }
                    int rid = 0;
                    int tcid = 0;
                    int cid = 0;
                    int minTemplateID = 0;
                    agreedValueContract.BreakClauses.ForEach(b =>
                    {
                        b.BreakClauseID = 0;
                    });
                    agreedValueContract.CustomFieldValues.ForEach(v =>
                    {
                        v.CustomFieldValueID = 0;
                    });
                    agreedValueContract.ExitCosts.ForEach(e =>
                    {
                        e.ContractID = contract.ContractID;
                        e.ID = 0;
                    });
                    agreedValueContract.Guarantees.ForEach(g =>
                    {
                        g.GuaranteeID = 0;
                        g.Guarantors.ForEach(g2 =>
                        {
                            g2.GuaranteeGuarantorID = 0;
                        });
                    });
                    agreedValueContract.Incentives.ForEach(v =>
                    {
                        v.ContractID = contract.ContractID;
                        v.ID = 0;
                    });
                    agreedValueContract.InitialCosts.ForEach(ic =>
                    {
                        ic.ContractID = contract.ContractID;
                        ic.ID = 0;
                    });
                    agreedValueContract.MakeGoodCosts.ForEach(m =>
                    {
                        m.ContractID = contract.ContractID;
                        m.ID = 0;
                    });
                    agreedValueContract.Terms.ForEach(t =>
                    {
                        t.ContractID = contract.ContractID;
                    });
                    agreedValueContract.ParentContracts.ForEach(c =>
                    {
                        c.ContractID = contract.ContractID;
                        c.ID = 0;
                    });
                    agreedValueContract.AssetSchedule.ForEach(c =>
                    {
                        c.ContractID = contract.ContractID;
                        c.ID = 0;
                    });
                    agreedValueContract.OtherClauses.ToList().ForEach(o =>
                    {
                        o.ContractID = model.ContractID;
                        o.ContractClauseID = 0;
                        o.Amendments.ForEach(a =>
                        {
                            a.ContractClauseID = o.ContractClauseID;
                            a.AmendmentID = 0;
                        });
                        o.TriggeredRecords.ForEach(t =>
                        {
                            t.ContractClauseID = o.ContractClauseID;
                            t.RecordID = 0;
                        });
                    });

                    agreedValueContract.Invoices.ForEach(inv =>
                    {
                        inv.LinkedContractID = contract.ContractID;
                        inv.InvoiceID = 0;
                        Guid newInvoiceEntity = Guid.NewGuid();
                        Guid oldInvoiceEnity = inv.EntityID;
                        var itype = invoiceTypeService.GetInvoiceTypes()
                            .FirstOrDefault(s => (s.Direction != 'R' && !inv.IsReceivable) || (s.Direction != 'P' && inv.IsReceivable) || s.Direction == 'B');
                        List<CustomFieldValueEditModel> invCustomValues = extendableService.GetExtendableEntityValuesEdit(oldInvoiceEnity, itype.EntityID, "InvoiceType", inv.IsReceivable ? "Receivable" : "Payable", false);
                        var invEntity = MappingContext.Instance.Map<List<CustomFieldValueEditModel>, IEnumerable<CustomFieldValue>>(invCustomValues);
                        CustomFieldValuesEntityDict.Add(newInvoiceEntity, invEntity);
                        inv.EntityID = newInvoiceEntity;
                        inv.Costs.ForEach(c =>
                        {
                            c.EntityID = Guid.NewGuid();
                            c.InvoiceID = 0;
                            c.CostID = 0;
                            c.Splits.ForEach(s =>
                            {
                                s.SplitID = 0;
                            });
                        });
                    });

                    Dictionary<string, int> costdict = new Dictionary<string, int>();
                    agreedValueContract.Templates.ForEach(t =>
                    {
                        t.ContractID = contract.ContractID;
                        t.InvoiceTemplateID = --minTemplateID;
                        t.EntityID = Guid.NewGuid();
                        t.Costs.ForEach(c =>
                        {
                            c.TemplateCostId = --tcid;
                            c.InvoiceTemplateID = t.InvoiceTemplateID;
                            var key = c.AssetID + "-" + c.Description + "-" + c.CategoryID;
                            costdict.Add(key, c.TemplateCostId);
                        });
                    });

                    agreedValueContract.Reviews.ForEach(r =>
                    {
                        r.ContractID = contract.ContractID;
                        r.ReviewID = --rid;
                        r.Costs.ForEach(c =>
                        {
                            c.CostID = 0;
                        });
                        if (r.ActionedReview != null)
                        {
                            r.ActionedReview.ReviewID = r.ReviewID;
                            r.ActionedReview.Costs.ForEach(c =>
                            {
                                c.CostID = --cid;
                                var key = c.AssetID + "-" + c.Label + "-" + c.CategoryID;
                                c.TemplateCostID = c.TemplateCostID == null ? c.TemplateCostID : costdict[key];
                                c.ReviewID = r.ReviewID;
                            });
                        }
                    });

                    contractService.CreateContract(agreedValueContract, journal);
                    int contextAsset = agreedValueContract.Assets().First();
                    //insert new custom values for new invoices
                    foreach (var i in CustomFieldValuesEntityDict)
                    {
                        if (i.Value.Count() > 0)
                        {
                            extendableService.SaveCustomFieldValuesForEntity(i.Key, i.Value);
                        }
                    }
                    return ExtendedJson(new
                    {
                        success = true,
                        message = string.Format("Clone into new contract successfully"),
                        Url = Url.AssetTabAction("Detail", contextAsset, "contracts", "view", new { contractid = agreedValueContract.ContractID })
                    });
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch (DomainValidationException ex)
            {
                DeleteAddedAssets(newAssetsIds);
                return ExtendedJson(new
                {
                    success = false,
                    message = ex.Errors.Select(e => e.Message).Distinct()
                });
            }
            catch (DomainSecurityException dex)
            {
                DeleteAddedAssets(newAssetsIds);
                return ExtendedJson(new
                {
                    success = false,
                    message = dex.Message
                });
            }
            catch (Exception)
            {
                DeleteAddedAssets(newAssetsIds);
                return ExtendedJson(new
                {
                    success = false,
                    message = string.Format("Clone into new contract failed"),
                });
            }
        }

        private void DeleteAddedAssets(int[] newAssetsIds)
        {
            for (int i = 0; i < newAssetsIds.Length; i++)
            {
                assetService.DeleteAsset(newAssetsIds[i]);
            }
        }

        /// <summary>
        /// The SetOptionState.
        /// </summary>
        /// <param name="ContractID">The ContractID<see cref="int"/>.</param>
        /// <param name="TermStart">The TermStart<see cref="DateTime"/>.</param>
        /// <param name="newState">The newState<see cref="string"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult SetOptionState(int ContractID, DateTime TermStart, string newState)
        {

            if (!new string[] { "Pending", "In Progress" }.Contains(newState))
            {
                return ExtendedJson(new { success = false, message = $"The Option state could not be set to {newState}." });
            }
            ContractEditModel contract = contractService.GetContractEdit(ContractID);
            if (contract == null)
            {
                return ExtendedJson(new { success = false, message = "Contract could not be found and may have been deleted." });
            }
            TermEditModel term = contract.Terms.SingleOrDefault(r => r.TermStart == TermStart);

            if (term != null && TrySave(() =>
            {
                term.State = newState;
                contractService.UpdateContract(contract, $"Set Option State of Term {TermStart.ToString(UserContext.Current.DateFormat)} to {newState}");
            }))
            {
                return ExtendedJson(new { success = true });
            }
            return ExtendedJson(new { success = false, message = "Could not update the Option State. The contract state may have been changed. Please reload the page and try again." });
        }

        /// <summary>
        /// The SetReviewState.
        /// </summary>
        /// <param name="ContractID">The ContractID<see cref="int"/>.</param>
        /// <param name="ReviewID">The ReviewID<see cref="int"/>.</param>
        /// <param name="newState">The newState<see cref="string"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult SetReviewState(int ContractID, int ReviewID, string newState)
        {
            if (!new string[] { "Pending", "In Progress" }.Contains(newState))
            {
                return ExtendedJson(new { success = false, message = $"The Review state could not be set to {newState}." });
            }
            AgreedValueContractEditModel contract = contractService.GetContractEdit(ContractID) as AgreedValueContractEditModel;
            if (contract == null)
            {
                return ExtendedJson(new { success = false, message = "Contract could not be found and may have been deleted." });
            }
            AgreedValueReviewEditModel review = contract.Reviews.SingleOrDefault(r => r.ReviewID == ReviewID);
            if (review != null && TrySave(() =>
            {
                review.State = newState;
                contractService.UpdateContract(contract, $"Set the state of the review on {review.ReviewDate.ToString(UserContext.Current.DateFormat)} to {newState}");
            }))
            {
                return ExtendedJson(new { success = true });
            }
            return ExtendedJson(new { success = false, message = "Could not update the review state. The contract state may have been changed. Please reload the page and try again." });
        }

        /// <summary>
        /// The SubContract.
        /// </summary>
        /// <param name="ContractID">The ContractID<see cref="int"/>.</param>
        /// <param name="model">The model<see cref="VMSubContractViewModel"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult SubContract(int ContractID, VMSubContractViewModel model)
        {
            ViewBag.ContractID = ContractID;
            ViewBag.ContextID = ContextAssetID;
            switch (model.Page.ToLower())
            {
                case "addsubcontract":
                    return AddEditSubcontractMappings(ContractID, null);

                case "editsubcontractmappings":
                    return AddEditSubcontractMappings(ContractID, model.SubContractID);

                case "editsubcontract":
                    return AddEditSubcontractMappings(ContractID, model.SubContractID);

                case "editcontract":
                    return EditContract(model.SubContractID);

                case "subcontractlist":
                case "":
                    return SubContractList(ContractID);

                case "parentcontracts":
                    return ParentContractList(ContractID);
            }
            return RedirectToAction("Partial", "Error", new { message = "The Contract you tried to view could not be found." });
        }

        /// <summary>
        /// The SubContractList.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult SubContractList(int ID)
        {
            List<SubContractMappingEditModel> subcontracts = contractService.GetSubContractsForContract(ID);
            AgreedValueContractEditModel contract = contractService.GetContractEdit(ID) as AgreedValueContractEditModel;

            //behaviour for FindMatchingAssets is if there are no matches is always return the assets in the second parameter. Pass in a GUID string so that it'll return nothing as term search term
            List<SubContractListViewModel> defaultSubContracts = assetService.FindMatchingAssets("", contract.Assets().ToArray())
                .Select(a => SimpleMapper.Map<AssetListModel, AssetViewModel>(a))
                .Select(a => new SubContractListViewModel
                {
                    Asset = a,
                    ParentContract = contract,
                    SubContractMappings = new List<SubContractMappingEditModel>()
                }).ToList();

            foreach (SubContractMappingEditModel mapping in subcontracts)
            {
                SubContractListViewModel matching = defaultSubContracts.Find(sc => sc.Asset.AssetID == mapping.AssetID);
                if (matching == null)
                {
                    matching = defaultSubContracts.Find(sc => sc.Asset.AssetID == mapping.Asset.ParentID);
                }

                matching.SubContractMappings.Add(mapping);
            }

            //contract.CurrentReview().ActionedReview.Costs.Select(c => new { c.AssetID, c.Asset }, )
            ViewBag.ContractID = ID;
            ViewBag.ContractName = contract.Description;
            return PartialView("Partial/SubContracts/SubcontractList", defaultSubContracts);
        }

        /// <summary>
        /// The UnlinkSubContract.
        /// </summary>
        /// <param name="subcontractid">The subcontractid<see cref="int"/>.</param>
        /// <param name="contractid">The contractid<see cref="int"/>.</param>
        /// <returns>The <see cref="JsonResult"/>.</returns>
        [HttpPost]
        public JsonResult UnlinkSubContract(int subcontractid, int contractid)
        {
            try
            {
                contractService.UnlinkSubContract(subcontractid, contractid);
                return Json(new { success = true });
            }
            catch (DomainValidationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch
            {
                return Json(new
                {
                    success = false
                });
            }
        }

        [HttpPost]
        public ExtendedJsonResult ShowArchivedDate(int Id, string state)
        {
            ContractEditModel contract = contractService.GetContractEdit(Id);
            ViewBag.contractId = Id;
            ViewBag.State = state;
            ViewBag.IsArchived = contract.IsArchived;
            return ExtendedJson(new
            {
                success = true,
                html = RenderVariantPartialViewToString("Dialog/ContractStatusDialog", contract),
            });
        }

        [HttpPost]
        public ActionResult EnterHoldoverDialog(int Id, string state)
        {
            try
            {
                ContractEditModel contract = contractService.GetContractEdit(Id);
                return ExtendedJson(new
                {
                    success = true,
                    html = RenderVariantPartialViewToString("Dialog/EnterHoldoverDialog", contract),
                });
            }
            catch (Exception ex)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }



        [HttpPost]
        public ExtendedJsonResult ShowUpdateAsset(int Id)
        {
            ContractViewModel contract = contractService.GetContractView(Id);
            ViewBag.contractId = Id;

            return ExtendedJson(new
            {
                success = true,
                html = RenderVariantPartialViewToString("Dialog/UpdateAssetDialog", contract),
            });
        }
        [HttpGet]
        public ActionResult TerminateContract(int ID)
        {
            try
            {
                var contractViewModel = contractService.GetTerminateContractDetails(ID);
                return PartialView("Dialog/TerminateContract", contractViewModel);
            }
            catch (Exception ex)
            {

                return
                    ExtendedJson(new
                    {
                        success = false,
                        message = ex.Message
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult TerminateContract(TerminateContractDetails model)
        {
            if (TryValidateModel(model))
            {
                try
                {
                    contractService.TerminateContract(model.ContractID, model.TerminationCost.Value, model.TerminationDate.Value);
                }
                catch (DomainValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                var errors = ModelState.SelectMany(ms => ms.Value.Errors).Select(e => e.ErrorMessage).ToList();
                if (errors.Count > 0)
                {
                    var msg = ModelState[""].Errors.FirstOrDefault();
                    return ExtendedJson(new
                    {
                        success = false,
                        message = msg,
                        html = RenderVariantPartialViewToString("Dialog/TerminateContract", model)
                    }, JsonRequestBehavior.DenyGet);
                }
                else
                {
                    var prior = leaseAccountingService.GetPriorLeaseAccountingReviews(model.ContractID, TimeSpan.MaxValue);
                    if (prior != null && prior.Count() > 0)
                    {
                        var contract = (AgreedValueContractEditModel)contractService.GetContractEdit(model.ContractID);
                        LeaseAccountingReviewEditModel draft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, true);

                        List<string> validationErrors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(draft, contract, new ValidationContext(draft)).Select(e => e.ErrorMessage).ToList();

                        if (validationErrors.Count == 0)
                        {
                            try
                            {
                                //exercising an option requires a formal lease accounting review
                                draft.IsFormalLeaseAccountingReview = true;
                                draft.TerminationCost = model.TerminationCost;
                                draft.TerminationDate = model.TerminationDate;
                                leaseAccountingService.UpdateLeaseAccountingReview(draft);
                                leaseAccountingService.SetLeaseAccountingReviewState(draft, "Submitted", LeaseAccountingReview_ProcessCode.RECORD_TERMINATION);
                                return ExtendedJson(new
                                {
                                    success = true
                                }, JsonRequestBehavior.DenyGet);
                            }
                            catch (DomainValidationException dex)
                            {
                                ModelState.AddModelError("security", dex.Message);
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError("", ex.Message);
                            }
                        }
                        else
                        {
                            throw new DomainValidationException(string.Join(", ", validationErrors.Distinct()));
                        }
                        var err = ModelState.SelectMany(ms => ms.Value.Errors).Select(e => e.ErrorMessage).ToList();
                        if (err.Count > 0)
                        {
                            contractService.RollbackContractTermination(model.ContractID);
                            var msg = ModelState[""].Errors.FirstOrDefault();
                            return ExtendedJson(new
                            {
                                success = false,
                                message = msg,
                                html = RenderVariantPartialViewToString("Dialog/TerminateContract", model)
                            }, JsonRequestBehavior.DenyGet);
                        }

                    }
                }
            }
            //if we made it here we got an error
            var m = ModelState[""].Errors.FirstOrDefault();
            return ExtendedJson(new
            {
                success = false,
                message = m,
                html = RenderVariantPartialViewToString("Dialog/TerminateContract", model)
            }, JsonRequestBehavior.DenyGet);
        }

        [HttpGet]
        public ActionResult RollbackTermination(int ID)
        {
            var contractView = contractService.GetContractView(ID);
            if (contractView == null)
            {
                return
                    ExtendedJson(new
                    {
                        success = false,
                        message = "Contract does not exist."
                    }, JsonRequestBehavior.AllowGet);
            }
            if (!contractView.TerminationDate.HasValue)
            {
                return
                    ExtendedJson(new
                    {
                        success = false,
                        message = "Contract is not currently terminated"
                    }, JsonRequestBehavior.AllowGet);

            }
            return PartialView("Dialog/RollbackTermination", contractView);
        }

        /// <summary>
        /// Does the rollback termination. Throws an exception if contract is not terminated
        /// </summary>
        /// <param name="ID">Contract ID</param>
        /// <param name="doRollback">This is only here so that the httpGet Request has a different method signature to this</param>
        /// <returns></returns>
        /// <exception cref="DomainValidationException"></exception>
        [HttpPost]
        public ExtendedJsonResult RollbackTermination(int ID, bool doRollback)
        {
            var initial = contractService.GetContractByID(ID);
            try
            {
                contractService.RollbackContractTermination(ID);
                var prior = leaseAccountingService.GetPriorLeaseAccountingReviews(ID, TimeSpan.MaxValue);
                if (prior != null && prior.Count() > 0)
                {
                    var contract = (AgreedValueContractEditModel)contractService.GetContractEdit(ID);
                    LeaseAccountingReviewEditModel draft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, true);

                    List<string> validationErrors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(draft, contract, new ValidationContext(draft)).Select(e => e.ErrorMessage).ToList();
                    if (validationErrors.Count > 0)
                    {
                        throw new DomainValidationException(string.Join(", ", validationErrors.Distinct()));
                    }

                    //exercising an option requires a formal lease accounting review
                    draft.IsFormalLeaseAccountingReview = true;
                    leaseAccountingService.UpdateLeaseAccountingReview(draft);
                    leaseAccountingService.SetLeaseAccountingReviewState(draft, "Submitted", LeaseAccountingReview_ProcessCode.ROLLBACK_TERMINATION);
                }
                return ExtendedJson(new
                {
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                if (initial.TerminationDate.HasValue)
                {
                    contractService.TerminateContract(initial.ContractID, initial.TerminationCost.Value, initial.TerminationDate.Value);
                }
                return ExtendedJson(new
                {
                    success = false,
                    message = ex.Message,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The UpdateContractState.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="state">The state<see cref="string"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult UpdateContractState(int ID, string state, DateTime? archiveDate)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return JsonUnauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return JsonUnauthorized();
            }

            ContractViewModel vm = contractService.GetContractView(ID);
            if (vm.IsReadOnly)
            {
                return ExtendedJson(new { success = false, message = "You do not have sufficient access to change the contract state of this contract" });
            }

            try
            {
                switch (state)
                {
                    case "archive":
                        contractService.SetContractArchiveState(ID, true, archiveDate);
                        return ExtendedJson(new { success = true, message = "Contract successfully archived" });

                    case "unarchive":
                        //Check if Asset is archived
                        AssetEditModel assetModel = assetService.GetAssetEdit(vm.Assets().First());
                        if (assetModel.Status == "Archived")
                        {
                            return ExtendedJson(new { success = false, message = "Unable to change contract state - Contract's asset is archived" });
                        }
                        {
                            contractService.SetContractArchiveState(ID, false, archiveDate);
                            return ExtendedJson(new { success = true, message = "Contract successfully de-archived" });
                        }

                    case "holdover":
                        contractService.SetContractHoldOverState(ID, true, archiveDate);

                        return ExtendedJson(new { success = true, message = "Contract holdover period successfully entered" });

                    case "exitholdover":
                        contractService.SetContractHoldOverState(ID, false);
                        return ExtendedJson(new { success = true, message = "Contract holdover period successfully ended" });

                    case "break":
                        DateTime terminationDate;
                        if (DateTime.TryParseExact(Request.Params["TerminationDate"], UserContext.Current.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out terminationDate))
                        {
                            try
                            {

                                contractService.BreakContract(ID, terminationDate);

                                return ExtendedJson(new { success = true, message = "Contract successfully terminated" });
                            }
                            catch (DomainEntityNotFoundException)
                            {
                                return ExtendedJson(new { success = false, message = "Unable to terminate contract - the contract does not exist" });
                            }
                            catch (DomainValidationException dve)
                            {
                                return ExtendedJson(new { success = false, message = "Unable to terminate contract - " + dve.Message });
                            }
                        }
                        else
                        {
                            return ExtendedJson(new { success = false, message = "Unable to break contract - invalid termination date" });
                        }

                    case "unbreak":
                        try
                        {
                            contractService.UnbreakContract(ID);
                            return ExtendedJson(new { success = true, message = "Contract break option successfully un-exercised" });
                        }
                        catch (DomainEntityNotFoundException)
                        {
                            return ExtendedJson(new { success = false, message = "Unable to break contract - the contract does not exist" });
                        }
                        catch (DomainValidationException dve)
                        {
                            return ExtendedJson(new { success = false, message = "Unable to break contract - " + dve.Message });
                        }

                    default:
                        return ExtendedJson(new { success = false, message = "Unable to change contract state - unknown state change requested" });
                }
            }
            catch (DomainEntityNotFoundException)
            {
                return ExtendedJson(new { success = false, message = "Unable to change contract state - the contract does not exist" });
            }
            catch (DomainValidationException dve)
            {
                return ExtendedJson(new { success = false, message = dve.Message });
            }
            catch (Exception)
            {
                return ExtendedJson(new { success = false, message = "Unable to change contract state - an unknown error occured" });
            }
        }

        public ExtendedJsonResult ConfirmEvergreen(int ID)
        {
            AgreedValueContractEditModel contract = contractService.GetContractEdit(ID, false) as AgreedValueContractEditModel;
            List<LeaseAccountingSyncStatusModel> LeaseAccountingSyncStatus =
                        leaseAccountingService.GetLeaseAccountingReviewSynchronisationStatusByContract(ID);
            if (contract.CurrentEndingTerm().TermEnd.HasValue)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The contract has an end date and cannot be marked as evergreen, please update the contract first."
                }, JsonRequestBehavior.AllowGet);
            }
            if (LeaseAccountingSyncStatus.Count > 0)
            {
                LeaseAccountingSyncStatusModel LastLeaseAccountingSyncStatus =
               LeaseAccountingSyncStatus.OrderByDescending(r => r.CreatedDate).FirstOrDefault();
                string lastEvent = LastLeaseAccountingSyncStatus.LAP_EventCode;
                //sanity check. They shouldn't be able to get here
                if (!new string[] { "ACCT_APPROVED", "RE_REJECTED", "ACCT_REJECTED" }.Contains(lastEvent))
                {
                    throw new Exception("Contract has been updated by another user. Please refresh and try again");
                }

                LeaseAccountingReviewEditModel review = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, true, true);
                review.IsFormalLeaseAccountingReview = true;
                if (!TrySave(() => leaseAccountingService.UpdateLeaseAccountingReview(review)))
                {
                    throw new ValidationException("Unable to mark as evergreen, please try again");
                }
                try
                {
                    leaseAccountingService.SetLeaseAccountingReviewState(review, "Submitted", LeaseAccountingReview_ProcessCode.EVERGREEN);
                    return ExtendedJson(new
                    {
                        success = true,
                    }, JsonRequestBehavior.AllowGet);
                }
                catch (LeaseAcceleratorImportValidationException ex)
                {

                    return ExtendedJson(new
                    {
                        success = false,
                        message = ex.Message
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The contract has not been synchronized and cannot be marked as evergreen."
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// The UpdateDates.
        /// </summary>
        /// <param name="templates">The templates<see cref="List{TemplateDateUpdateModel}"/>.</param>
        /// <param name="effectiveDate">The effectiveDate<see cref="DateTime"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult UpdateDates(List<TemplateDateUpdateModel> templates, DateTime effectiveDate)
        {
            templates.ForEach(t =>
            {
                t.FirstInvoiceDate = AdvanceToDate(t.FirstInvoiceDate, effectiveDate, t.Frequency, t.Pattern, true);
                t.ActionedCosts.ForEach(c => c.FirstPaymentDate = AdvanceToDate(c.FirstPaymentDate, t.FirstInvoiceDate, c.Frequency, c.Pattern, true));
                t.UnactionedCosts.ForEach(c => c.FirstPaymentDate = AdvanceToDate(c.FirstPaymentDate, t.FirstInvoiceDate, c.Frequency, c.Pattern, true));
            });
            return ExtendedJson(templates);
        }

        /// <summary>
        /// The UpdateParentContractMapping.
        /// </summary>
        /// <param name="ParentContractMappings">The ParentContractMappings<see cref="List{SubContractMappingEditModel}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult UpdateParentContractMapping(List<SubContractMappingEditModel> ParentContractMappings)
        {
            try
            {
                contractService.UpdateParentContractMappings(ParentContractMappings);
                //_contract.

                return Json(new { success = true });
            }
            catch (DomainValidationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// The UpdateSubContractMappings.
        /// </summary>
        /// <param name="SubContractMappings">The SubContractMappings<see cref="List{SubContractMappingEditModel}"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        [HttpPost]
        public ActionResult UpdateSubContractMappings(List<SubContractMappingEditModel> SubContractMappings)
        {
            try
            {
                contractService.UpdateSubContractMappings(SubContractMappings);
                //_contract.

                return Json(new { success = true });
            }
            catch (DomainValidationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// The UploadDocument.
        /// </summary>
        /// <param name="filekey">The filekey<see cref="string"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        [HttpPost]
        public ExtendedJsonResult UploadDocument(string filekey)
        {
            string destination = Request.Form["destination"];

            switch (destination)
            {
                case "cmis":
                    string[] entries = Request.Form.GetValues("entries");
                    string precis = Request.Form["precis"];
                    int classification = int.Parse(Request.Form["classification"]);
                    byte[] bytes;
                    string filename;

                    if (filekey == null)
                    {
                        HttpPostedFileBase file = Request.Files["file"];
                        if (file == null)
                        {
                            return ExtendedJson(new
                            {
                                success = false,
                                message = "The document failed to upload successfullly"
                            });
                        }

                        filename = file.FileName;
                        using (MemoryStream tempStream = new MemoryStream())
                        {
                            file.InputStream.CopyTo(tempStream);
                            bytes = tempStream.ToArray();
                        }
                    }
                    else
                    {
                        bytes = System.IO.File.ReadAllBytes(Path.GetTempPath() + filekey + ".docx");
                        filename = System.IO.File.ReadAllText(Path.GetTempPath() + filekey + ".def");
                    }
                    (bool IsInValidFileExtension, string errorMessage) = fileService.ValidateFileExtension(filename);
                    if (IsInValidFileExtension)
                        return ExtendedJson(new { success = false, message = errorMessage });


                    ECMISActionResult result = documentService.UploadDocumentViaCMIS(bytes, filename, entries, classification, UserContext.Current.DisplayName, precis, ClientContext.Current.GetConfigurationSetting("CMIS.RegisterDocuments", true));
                    switch (result)
                    {
                        case ECMISActionResult.SUCCESS:
                            return ExtendedJson(new { success = true, message = (filename.EndsWith(".docx") ? filename : filename + ".docx") + " uploaded to repository and linked to topic index." });

                        case ECMISActionResult.BAD_CONFIG_MISSING_DOC:
                        case ECMISActionResult.BAD_CONFIG_MISSING_PWD:
                        case ECMISActionResult.BAD_CONFIG_MISSING_REPO:
                        case ECMISActionResult.BAD_CONFIG_MISSING_SALT:
                        case ECMISActionResult.ERR_CREATING_SESSION:
                        case ECMISActionResult.ERR_CREATING_SHELL_DOC:
                        case ECMISActionResult.ERR_FINALIZING_CONTENT:
                        case ECMISActionResult.ERR_LINKING_INDEX:
                        case ECMISActionResult.ERR_NEW_OBJECTID:
                        case ECMISActionResult.ERR_REGISTERING:
                        case ECMISActionResult.ERR_SET_CLASS:
                        case ECMISActionResult.ERR_STREAMING_CONTENT:
                        case ECMISActionResult.ERR_UNKNOWN:
                            return ExtendedJson(new { success = false, message = "File could not be uploaded to repository (CMIS" + ((int)result).ToString("D3") + ")" });

                        default:
                            return ExtendedJson(new { success = false, message = "File could not be uploaded to repository (Access Denied)" });
                    }
            }
            return ExtendedJson(new
            {
                success = false,
                message = "Unknown document destination - please try again"
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Validate that the specified asset has not already been added to another contract asset schedule.
        /// </summary>
        /// <param name="id">.</param>
        /// <param name="assetId">.</param>
        /// <returns>.</returns>
        public ExtendedJsonResult ValidateAssetScheduleAddition(int id, int assetId, int contractid)
        {
            string error = contractService.ValidateAssetScheduleAddition(id, assetId, contractid);
            return ExtendedJson(new
            {
                success = string.IsNullOrWhiteSpace(error),
                message = error
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The ValidateLeaseAccountingReview.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult ValidateLeaseAccountingReview(int ID)
        {
            if (!UserContext.Current.EvaluateAccess(true,
                TestAssetIsAccessible,
                LeaseAccountingReviewPermissions.Create))
            {
                return JsonUnauthorized();
            }

            LeaseAccountingReviewEditModel review = leaseAccountingService.GetLeaseAccountingReviewEdit(ID);
            AgreedValueContractEditModel contract = contractService.GetContractEdit(review.ContractID) as AgreedValueContractEditModel;
            if (review == null)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " review does not exist and may have been removed by another user"
                });
            }
            LeaseAccountingReviewEditModel currentDraft = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, true, true);
            if (currentDraft.LeaseAccountingReviewID != ID)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "The current " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " Review is no longer valid. has been made. Please restart the wizard."
                });
            }

            List<ValidationResult> errors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(review, contract, new ValidationContext(review)).ToList();

            return ExtendedJson(new
            {
                success = errors.Count < 1,
                errors = errors.Select(e => e.ErrorMessage).Distinct()
            });
        }

        /// <summary>
        /// The VerifyLeaseAccountingAccountCode.
        /// </summary>
        /// <param name="code">The code<see cref="string"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        public ExtendedJsonResult VerifyLeaseAccountingAccountCode(string code)
        {
            return ExtendedJson(new
            {
                success = LoisProvider.IsEnabled ? LoisProvider.ValidateAccountCode(code) : true
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// The ViewAssetScheduleItem.
        /// </summary>
        /// <param name="assetID">The assetID<see cref="int"/></param>
        /// <returns>The <see cref="ActionResult"/></returns>
        public ActionResult ViewAssetScheduleItem(int assetID, int contractID)
        {
            AssetEditModel assetModel = assetService.GetAssetEdit(assetID);
            //we dont care about primary asset and id. this is just for display purposes

            AgreedValueContractEditModel contract = contractService.GetContractEdit(contractID) as AgreedValueContractEditModel;
            ContractAssetScheduleItemEditModel assetSchedule = contract.AssetSchedule.FirstOrDefault(a => a.AssetID == assetID);

            ContractAssetScheduleItemEditModel model = new ContractAssetScheduleItemEditModel
            {
                Asset = assetModel.Name,
                AvailableForUseDate = assetSchedule.AvailableForUseDate,
                DepreciationStartDate = assetSchedule.DepreciationStartDate,
                CostCenter = assetSchedule.CostCenter,
                GLCode = assetSchedule.GLCode,
                UnitPrice = assetSchedule.UnitPrice,
                AssetOwner = assetSchedule.AssetOwner,
                AssetOwnerID = assetSchedule.AssetOwnerID,
                AssetUser = assetSchedule.AssetUser,
                AssetUserID = assetSchedule.AssetUserID,
                BusinessUnit = assetModel.BusinessUnit,
                BusinessUnitID = assetModel.BusinessUnitID,
                LegalEntity = assetModel.LegalEntity,
                LegalEntityID = assetModel.LegalEntityID,
                ValidFrom = assetSchedule.ValidFrom,
                ValidTo = assetSchedule.ValidTo
            };
            return PartialView("DisplayTemplates/ContractAssetScheduleItemEditModel", model);
        }

        ///// <summary>
        ///// The EditAssetScheduleItem.
        ///// </summary>
        ///// <param name="assetID">The assetID<see cref="int"/></param>
        ///// <returns>The <see cref="ActionResult"/></returns>
        //public ActionResult EditAssetScheduleItem(int assetID, int contractID)
        //{
        //    AssetEditModel assetModel = assetService.GetAssetEdit(assetID);
        //    //we dont care about primary asset and id. this is just for display purposes

        //    AgreedValueContractEditModel contract = contractService.GetContractEdit(contractID) as AgreedValueContractEditModel;
        //    ContractAssetScheduleItemEditModel assetSchedule = contract.AssetSchedule.FirstOrDefault(a => a.AssetID == assetID);

        //    ContractAssetScheduleItemEditModel model = new ContractAssetScheduleItemEditModel
        //    {
        //        ID = assetSchedule.ID,
        //        Asset = assetModel.Name,
        //        AvailableForUseDate = assetSchedule.AvailableForUseDate,
        //        DepreciationStartDate = assetSchedule.DepreciationStartDate,
        //        CostCenter = assetSchedule.CostCenter,
        //        GLCode = assetSchedule.GLCode,
        //        UnitPrice = assetSchedule.UnitPrice,
        //        AssetOwner = assetSchedule.AssetOwner,
        //        AssetOwnerID = assetSchedule.AssetOwnerID,
        //        AssetUser = assetSchedule.AssetUser,
        //        AssetUserID = assetSchedule.AssetUserID,
        //        BusinessUnit = assetModel.BusinessUnit,
        //        BusinessUnitID = assetModel.BusinessUnitID,
        //        LegalEntity = assetModel.LegalEntity,
        //        LegalEntityID = assetModel.LegalEntityID,
        //        ValidFrom = assetSchedule.ValidFrom,
        //        ValidTo = assetSchedule.ValidTo
        //    };
        //    return PartialView("EditorTemplates/ContractAssetScheduleItemEditModel", model);
        //}

        /// <summary>
        /// The ViewContract.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <param name="contextID">The contextID<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ViewContract(int ID, int contextID)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Base))
            {
                return PartialUnauthorized();
            }

            try
            {
                ViewBag.ContextID = contextID;
                ViewBag.ContractID = ID;
                ContractViewModel model = contractService.GetContractByID(ID);
                return PartialView("ViewContract", model);
            }
            catch (DomainEntityNotFoundException)
            {
                return RedirectToAction("Partial", "Error", new { message = "The Contract you tried to view could not be found." });
            }
        }

        /// <summary>
        /// The ViewContractDetails.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ViewContractDetails(int ID)
        {
            try
            {
                ContractViewModel contractview = contractService.GetContractView(ID);
                int contextAsset = contractview.Assets().First();
                return Redirect(Url.AssetTabAction("Detail", contextAsset, "contracts", "view", new { contractid = ID }));
            }
            catch (DomainEntityNotFoundException)
            {
                return RedirectToAction("Partial", "Error", new { message = "The Contract you tried to view could not be found." });
            }
        }

        /// <summary>
        /// The ViewContractVendorHistory.
        /// </summary>
        /// <param name="contractID">The contractID<see cref="int"/>.</param>
        /// <returns>The <see cref="ActionResult"/>.</returns>
        public ActionResult ViewContractVendorHistory(int contractID)
        {
            ViewBag.AllowEdit = false;
            IList<ContractVendorHistoryEditModel> history = contractService.GetContractVendorHistory(contractID);
            return PartialView("Dialog/ContractVendorHistory", history);
        }

        /// <summary>
        /// The ViewLeaseAccountingReview.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult ViewLeaseAccountingReview(int ID)
        {
            if (!UserContext.Current.EvaluateAccess(true, TestAssetIsAccessible, LeaseAccountingReviewPermissions.Landing))
            {
                return PartialUnauthorized();
            }

            LeaseAccountingReviewEditModel review = leaseAccountingService.GetLeaseAccountingReviewEdit(ID);
            //ContractViewModel contract = contractService.GetContractView(review.ContractID);
            var currency = localeService.GetCurrencyByAbbreviation(review.CurrencyAbbreviation);
            ViewBag.CurrencyFormat = currency.FormatString;
            ViewBag.LeaseAccountingLedgerSystems = leaseAccountingService.GetLedgerSystems();
            ViewBag.LeaseAccountingHiddenFields = ClientContext.Current.GetConfigurationSetting("LeaseAccounting.Fields.Hidden", "").Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
            return PartialView("Tabs/WizardPages/Dialog/ViewLeaseAccountingReview", review);
        }

        /// <summary>
        /// The ViewLeaseAccountingReviewList.
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/>.</param>
        /// <returns>The <see cref="PartialViewResult"/>.</returns>
        public PartialViewResult ViewLeaseAccountingReviewList(int ID)
        {
            if (!UserContext.Current.EvaluateAccess(true, TestAssetIsAccessible, LeaseAccountingReviewPermissions.Landing))
            {
                return PartialUnauthorized();
            }

            ContractViewModel contract = contractService.GetContractView(ID);
            ViewBag.CurrencyFormat = contract.CurrencyFormat;
            IEnumerable<LeaseAccountingReviewEditModel> reviews = leaseAccountingService.GetPriorLeaseAccountingReviews(ID, TimeSpan.MaxValue);

            return PartialView("Tabs/WizardPages/Dialog/ViewLeaseAccountingReviewList", reviews);
        }

        /// <summary>
        /// The AdvanceDate.
        /// </summary>
        /// <param name="date">The date<see cref="DateTime"/>.</param>
        /// <param name="frequency">The frequency<see cref="int"/>.</param>
        /// <param name="pattern">The pattern<see cref="string"/>.</param>
        /// <returns>The <see cref="DateTime"/>.</returns>
        private static DateTime AdvanceDate(DateTime date, int frequency, string pattern)
        {
            switch (pattern)
            {
                case "Weeks":
                    return date.AddDays(frequency * 7);

                case "Months":
                    return date.AddMonths(frequency);

                case "Quarters":
                    return date.AddMonths(frequency * 3);

                case "Years":
                    return date.AddYears(frequency);

                default:
                    return date.AddMonths(frequency);
            }
        }

        /// <summary>
        /// The AdvanceToDate.
        /// </summary>
        /// <param name="starting">The starting<see cref="DateTime"/>.</param>
        /// <param name="date">The date<see cref="DateTime"/>.</param>
        /// <param name="frequency">The frequency<see cref="int"/>.</param>
        /// <param name="pattern">The pattern<see cref="string"/>.</param>
        /// <param name="allowExact">The allowExact<see cref="bool"/>.</param>
        /// <returns>The <see cref="DateTime"/>.</returns>
        private static DateTime AdvanceToDate(DateTime starting, DateTime date, int frequency, string pattern, bool allowExact)
        {
            while ((allowExact && starting.Date < date.Date) || (!allowExact && starting.Date <= date.Date))
            {
                starting = AdvanceDate(starting.Date, frequency, pattern);
            }
            return starting.Date;
        }

        /// <summary>
        /// The FirstPaymentDate.
        /// </summary>
        /// <param name="starting">The Lease starting<see cref="DateTime"/>.</param>
        /// <param name="date">The cost date<see cref="DateTime"/>.</param>
        /// <param name="reviewDate">The review date<see cref="DateTime"/>.</param>
        /// <param name="frequency">The frequency<see cref="int"/>.</param>
        /// <param name="pattern">The pattern<see cref="string"/>.</param>
        /// <param name="allowExact">The allowExact<see cref="bool"/>.</param>
        /// <returns>The <see cref="DateTime"/>.</returns>
        private static DateTime FirstPaymentDateByPatternAndFrequency(DateTime starting, DateTime date, DateTime reviewDate, int frequency, string pattern, bool allowExact)
        {
            date = date.AdvanceUpToDateByPatternAndFrequency(reviewDate, pattern, frequency, true);
            if (DateTime.DaysInMonth(starting.Year, starting.Month) == starting.Day)
            {
                date = new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
            }
            return date.Date;
        }

        /// <summary>
        /// The FormatExerciseState.
        /// </summary>
        /// <param name="state">The state<see cref="LeaseAccountingReviewTermExerciseStates"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string FormatExerciseState(LeaseAccountingReviewTermExerciseStates state)
        {
            switch (state)
            {
                case LeaseAccountingReviewTermExerciseStates.Exercised:
                    return "Exercised";

                default:
                    return "~not set~";

                case LeaseAccountingReviewTermExerciseStates.ReasonablyCertainNotToBeExercised:
                    return "Reasonably Certain Not To Be Exercised";

                case LeaseAccountingReviewTermExerciseStates.ReasonablyCertainToBeExercised:
                    return "Reasonably Certain To Be Exercised";
            }
        }

        /// <summary>
        /// The GenerateAdminExcelLog.
        /// </summary>
        /// <param name="auditEntries">The auditEntries<see cref="IEnumerable{SystemAuditLogEntry}"/>.</param>
        /// <returns>The <see cref="ExcelPackage"/>.</returns>
        private static ExcelPackage GenerateAdminExcelLog(IEnumerable<SystemAuditLogEntry> auditEntries)
        {
            ExcelPackage package = new ExcelPackage();

            ExcelWorksheet auditSheet = package.Workbook.Worksheets.Add("Audit Entries");

            // Header row
            auditSheet.Cells[1, 1].Value = "Date";
            auditSheet.Cells[1, 2].Value = "User";
            auditSheet.Cells[1, 3].Value = "Name";
            auditSheet.Cells[1, 4].Value = "Type";
            auditSheet.Cells[1, 5].Value = "Sub Type";
            auditSheet.Cells[1, 6].Value = "Field";
            auditSheet.Cells[1, 7].Value = "Old Value";
            auditSheet.Cells[1, 8].Value = "New Value";

            using (ExcelRange range = auditSheet.Cells["A1:H1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189));
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
            }

            auditEntries = auditEntries.OrderBy(ae => ae.EntityID).ThenBy(ae => ae.EntityType).ThenBy(ae => ae.EntryDateTime);

            int crow = 2;
            foreach (SystemAuditLogEntry entry in auditEntries)
            {
                auditSheet.Cells[crow, 1].Value = entry.EntryDateTime.ToString();
                auditSheet.Cells[crow, 2].Value = entry.User + " (" + entry.Username + ")";
                auditSheet.Cells[crow, 3].Value = entry.EntityName;
                auditSheet.Cells[crow, 4].Value = entry.EntityType;
                auditSheet.Cells[crow, 5].Value = entry.EntitySubType;
                auditSheet.Cells[crow, 6].Value = entry.Item;
                auditSheet.Cells[crow, 7].Value = entry.ItemDetail;
                auditSheet.Cells[crow, 8].Value = entry.NewDetail;
                crow++;
            }

            return package;
        }

        /// <summary>
        /// The GenerateClauseHeirarchy.
        /// </summary>
        /// <param name="model">The model<see cref="ContractClauseEditModel"/>.</param>
        /// <returns>The <see cref="List{ClauseHeirarchy}"/>.</returns>
        private List<ClauseHierarchy> GenerateClauseHeirarchy(ContractClauseEditModel model)
        {
            // We are generating a hierarchy composed from predefined clauses, in use clauses, and the model clause

            // 1. Predefined clauses
            IEnumerable<PredefinedClauseViewModel> clauses = contractService.GetPredefinedClauses();
            List<ClauseHierarchy> hierarchy = clauses.GroupBy(c => c.Category).Select(g => new ClauseHierarchy
            {
                Category = g.Key,
                Clauses = g.ToList()
            }).ToList();

            // 2. In use clauses
            IEnumerable<Tuple<string, string>> existingClauses = contractService.GetInUseContractClauseCategoriesAndClauses();
            foreach (Tuple<string, string> clause in existingClauses)
            {
                ClauseHierarchy category = hierarchy.FirstOrDefault(cat => cat.Category == clause.Item1);
                if (category == null)
                {
                    hierarchy.Add(new ClauseHierarchy
                    {
                        Category = clause.Item1,
                        Clauses = new List<PredefinedClauseViewModel>
                        {
                            new PredefinedClauseViewModel
                            {
                                Category = clause.Item1,
                                Clause = clause.Item2,
                                PercentageFieldMode = FieldMode.Optional,
                                YearFieldMode = FieldMode.Optional,
                                AreaFieldMode = FieldMode.Optional,
                                AmountPayableMode = FieldMode.Optional,
                                AmountReceivableMode = FieldMode.Optional,
                                PayableToMode = FieldMode.Optional,
                                ReceivableFromMode = FieldMode.Optional
                            }
                        }
                    });
                }
                else
                {
                    if (!category.Clauses.Any(c => c.Clause == clause.Item2))
                    {
                        category.Clauses.Add(new PredefinedClauseViewModel
                        {
                            Category = clause.Item1,
                            Clause = clause.Item2,
                            PercentageFieldMode = FieldMode.Optional,
                            YearFieldMode = FieldMode.Optional,
                            AreaFieldMode = FieldMode.Optional,
                            AmountPayableMode = FieldMode.Optional,
                            AmountReceivableMode = FieldMode.Optional,
                            PayableToMode = FieldMode.Optional,
                            ReceivableFromMode = FieldMode.Optional
                        });
                    }
                }
            }

            // 3. Model clause
            if (model != null)
            {
                // make sure that any ad-hoc clause defined for the model is added to the hierarchy
                ClauseHierarchy category = hierarchy.FirstOrDefault(cat => cat.Category == model.Category);
                if (category == null)
                {
                    hierarchy.Add(new ClauseHierarchy
                    {
                        Category = model.Category,
                        Clauses = new List<PredefinedClauseViewModel> {
                            new PredefinedClauseViewModel {
                                Category = model.Category,
                                Clause = model.Clause,
                                PercentageFieldMode = FieldMode.Optional,
                                YearFieldMode = FieldMode.Optional,
                                AreaFieldMode = FieldMode.Optional,
                                AmountPayableMode = FieldMode.Optional,
                                AmountReceivableMode = FieldMode.Optional,
                                PayableToMode = FieldMode.Optional,
                                ReceivableFromMode = FieldMode.Optional
                            }
                        }
                    });
                }
                else if (!category.Clauses.Any(c => c.Clause == model.Clause))
                {
                    category.Clauses.Add(new PredefinedClauseViewModel
                    {
                        Category = model.Category,
                        Clause = model.Clause,
                        PercentageFieldMode = FieldMode.Optional,
                        YearFieldMode = FieldMode.Optional,
                        AreaFieldMode = FieldMode.Optional,
                        AmountPayableMode = FieldMode.Optional,
                        AmountReceivableMode = FieldMode.Optional,
                        PayableToMode = FieldMode.Optional,
                        ReceivableFromMode = FieldMode.Optional
                    });
                }
            }

            // 4. Sort the results nicely
            hierarchy.Sort((h1, h2) => h1.Category.CompareTo(h2.Category));
            hierarchy.ForEach(h =>
            {
                if (h.Clauses.Count > 1)
                {
                    h.Clauses = h.Clauses.Distinct().OrderBy(c => c.Clause).ToList();
                }
            });
            return hierarchy;
        }

        /// <summary>
        /// The GetLeaseAccountingReview.
        /// </summary>
        /// <param name="contract">The contract<see cref="AgreedValueContractEditModel"/>.</param>
        /// <returns>The <see cref="VMLeaseAccountingReviewEditModel"/>.</returns>
        private VMLeaseAccountingReviewEditModel GetLeaseAccountingReview(AgreedValueContractEditModel contract)
        {
            LeaseAccountingReviewEditModel previousLeaseAccountingReview = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewForContract(contract.ContractID, true, true);
            LeaseAccountingReviewEditModel draftLeaseAccountingReview = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, true, true);
            TermEditModel lastTerm = contract.Terms.OrderBy(t => t.TermStart).Last(t => t.State == "Exercised" || !t.IsOption);
            // now build the view model from the previous review and draft
            VMLeaseAccountingReviewEditModel model = ConvertToVMLeaseAccountingReviewEditModel(draftLeaseAccountingReview, contract, previousLeaseAccountingReview, lastTerm);

            // if there's a previous review changes will be determined by comparing contract to that review, else all terms on a contract are 'added'
            if (LoisProvider.IsEnabled)
            {
                int years = model.ProjectedEnd.YearsBetween(model.LeaseAccountingStartDate, true);
                Domain.Services.LeaseAccounting.Providers.Lois.DiscountRateError rateState = LoisProvider.GetDiscountRateForDuration(years, out decimal rate, out DateTime? expiry);
                if (rateState != Domain.Services.LeaseAccounting.Providers.Lois.DiscountRateError.NotFound)
                {
                    model.LatestDiscountRate = rate;
                    model.LatestDiscountRateExpiry = expiry;
                    model.DiscountRateSighted = draftLeaseAccountingReview.LeaseAccounting_DiscountRateSighted
                        && model.LatestDiscountRate == draftLeaseAccountingReview.LeaseAccounting_DiscountRate
                        && model.LatestDiscountRateExpiry.Value.Date >= DateTime.Today;
                }
                else
                {
                    model.LatestDiscountRate = 0M;
                    model.LatestDiscountRateExpiry = null;
                    model.DiscountRateSighted = false;
                }
            }
            model.TermChanges = GetLeaseAccountingTermChanges(previousLeaseAccountingReview, contract);
            return model;
        }

        /// <summary>
        /// The BulkParticipantUpdates
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/></param>
        /// <returns>The <see cref="PartialViewResult"/></returns>
        public PartialViewResult BulkParticipantUpdates(int ID)
        {
            AgreedValueContractEditModel contract = contractService.GetContractEdit(ID, false) as AgreedValueContractEditModel;

            BulkParticipantEditModel model = new BulkParticipantEditModel
            {
                ContractedPartyName = contract.ContractedParty,
                ContractedPartyID = contract.ContractedPartyID,
                ContractID = contract.ContractID,
                VendorID = contract.VendorID,
                VendorName = contract.Vendor,
                AssetSchedules = contract.AssetSchedule.Where(a => a.IsPrimaryAsset).ToList(),
                CurrencyID = contract.CurrencyID,
                VendorHistory = contract.VendorHistory
            };

            return PartialView("EditorTemplates/BulkParticipantEditModel", model);
        }

        /// <summary>
        /// The SaveBulkParticipantUpdates
        /// </summary>
        /// <param name="ID">The ID<see cref="int"/></param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        public ExtendedJsonResult SaveBulkParticipantUpdates(int ID, BulkParticipantEditModel model)
        {
            AgreedValueContractEditModel contract = contractService.GetContractEdit(ID, true) as AgreedValueContractEditModel;


            foreach (ContractAssetScheduleItemEditModel asset in model.AssetSchedules)
            {
                ContractAssetScheduleItemEditModel oldAsset = contract.AssetSchedule.First(a => a.AssetID == asset.AssetID);
                oldAsset.BusinessUnit = asset.BusinessUnit;
                oldAsset.BusinessUnitID = asset.BusinessUnitID;
                oldAsset.LegalEntity = asset.LegalEntity;
                oldAsset.LegalEntityID = asset.LegalEntityID;
                oldAsset.AssetOwner = asset.AssetOwner;
                oldAsset.AssetOwnerID = asset.AssetOwnerID;
                oldAsset.AssetUser = asset.AssetUser;
                oldAsset.AssetUserID = asset.AssetUserID;
            }
            contract.ContractedParty = model.ContractedPartyName;
            contract.ContractedPartyID = model.ContractedPartyID;
            contract.ContractID = model.ContractID;
            contract.CurrencyID = model.CurrencyID;
            contract.VendorHistory = model.VendorHistory;
            if (contract.VendorID != model.VendorID)
            {
                contactService.AddAccountingRoleToContact(model.VendorID, "Funder");
                contactService.AddAccountingRoleToContact(model.VendorID, "Vendor");
            }
            contract.VendorID = model.VendorID;
            contract.Vendor = model.VendorName;
            contract.CurrencyID = model.CurrencyID;
            contract.VendorHistory = model.VendorHistory;

            LeaseAccountingReviewEditModel lastsubmitted = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, true);
            lastsubmitted.IsFormalLeaseAccountingReview = true;

            List<string> validationErrors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(lastsubmitted, contract, new ValidationContext(lastsubmitted)).Select(e => e.ErrorMessage).ToList();
            if (validationErrors.Count > 0)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = validationErrors.Distinct()
                });
            }
            try
            {
                TemplateUpdateResult result = contractService.UpdateContract(contract, "Contract is being modified using the Bulk Participant Updates method");
                if (result != null)
                {
                    try
                    {
                        leaseAccountingService.SetLeaseAccountingReviewState(lastsubmitted, "Submitted", LeaseAccountingReview_ProcessCode.UPDATE_PARTICIPANTS);
                    }
                    catch (Exception ex)
                    {
                        return ExtendedJson(new
                        {
                            success = false,
                            message = ex.Message
                        }, JsonRequestBehavior.AllowGet);
                    }
                    return ExtendedJson(new
                    {
                        success = true,
                        message = "Contract's participants updated successfully"
                    });
                }
                else
                {
                    return ExtendedJson(new
                    {
                        success = false,
                        message = "Contract's participants failed to update"
                    });
                }
            }
            catch (DomainValidationException ex)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = ex.Errors.Select(e => e.Message).ToArray()
                });
            }
            catch (Exception ex)
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// The GetLeaseAccountingTermChanges
        /// </summary>
        /// <param name="previousLeaseAccountingReview">The previousLeaseAccountingReview<see cref="LeaseAccountingReviewEditModel"/>.</param>
        /// <param name="contract">The contract<see cref="AgreedValueContractEditModel"/>.</param>
        /// <returns>The <see cref="List{VMLeaseAccountingReviewTermChange}"/>.</returns>
        private List<VMLeaseAccountingReviewTermChange> GetLeaseAccountingTermChanges(LeaseAccountingReviewEditModel previousLeaseAccountingReview, AgreedValueContractEditModel contract)
        {
            List<LeaseAccountingReviewContractChangeListModel> changes = previousLeaseAccountingReview == null ? contract.Terms.OrderBy(t => t.TermStart).Select(t => new LeaseAccountingReviewContractChangeListModel
            {
                ChangeType = LeaseAccountingReviewContractChangeListModel.ChangeTypes.Added,
                Details = t.ToString(),
                Original = null,
                Section = LeaseAccountingReviewContractChangeListModel.Sections.Duration,
                Updated = t,
                UpdatedContext = t
            }).ToList() : LeaseAccountingProviderFactory.Current.GetContractChanges(previousLeaseAccountingReview, contract).Where(t => t.Section == LeaseAccountingReviewContractChangeListModel.Sections.Duration).ToList();
            changes = changes.GetFormalLeaseAccountingReviewTriggers();
            if (contract.LeaseAccounting_ForceReview ?? true)
            {
                changes.Add(new LeaseAccountingReviewContractChangeListModel
                {
                    ChangeType = LeaseAccountingReviewContractChangeListModel.ChangeTypes.Updated,
                    Section = LeaseAccountingReviewContractChangeListModel.Sections.Details,
                    Details = "Contract has been flagged as needing an " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " Review"
                });
            }
            return changes
                .OrderBy(t => t.Section == LeaseAccountingReviewContractChangeListModel.Sections.Details
                    || t.ChangeType == LeaseAccountingReviewContractChangeListModel.ChangeTypes.Removed ? t.OriginalAs<LeaseAccountingReviewTermEditModel>()?.TermStart : t.UpdatedAs<TermEditModel>()?.TermStart)
                .Select(t =>
                {
                    if (t.Section == LeaseAccountingReviewContractChangeListModel.Sections.Details)
                    {
                        return new VMLeaseAccountingReviewTermChange
                        {
                            Change = "Contract has been flagged as needing an " + LeaseAccountingOptions.Get<string>(LeaseAccountingOptions.LeaseAccountingReviewLabel) + " Review",
                            TermName = "N/A",
                            PreviousItems = new List<VMLeaseAccountingReviewItem> {
                                    new VMLeaseAccountingReviewItem {
                                        Label = "NA",
                                        Detail = "",
                                        Changed = true
                                    }
                                },
                            NewItems = new List<VMLeaseAccountingReviewItem>
                            {new VMLeaseAccountingReviewItem {
                                        Label = "NA",
                                        Detail = "",
                                        Changed = true
                                    }
                            }
                        };
                    }
                    LeaseAccountingReviewTermEditModel oldT = t.OriginalAs<LeaseAccountingReviewTermEditModel>();
                    TermEditModel newT = t.UpdatedAs<TermEditModel>();

                    switch (t.ChangeType)
                    {
                        case LeaseAccountingReviewContractChangeListModel.ChangeTypes.Added:
                            VMLeaseAccountingReviewTermChange o = new VMLeaseAccountingReviewTermChange
                            {
                                Change = "Added",
                                Duration = string.Format("{0:" + UserContext.Current.DateFormat + "} - {1}", newT.TermStart, newT.TermEnd.HasValue ? newT.TermEnd.Value.ToString(UserContext.Current.DateFormat) : "Open"),
                                PreviousItems = new List<VMLeaseAccountingReviewItem> {
                                    new VMLeaseAccountingReviewItem {
                                        Label = "NA",
                                        Detail = "",
                                        Changed = true
                                    }
                                },
                                NewItems = new List<VMLeaseAccountingReviewItem> {
                                new VMLeaseAccountingReviewItem {
                                    Label = "Exercise Window",
                                    Detail = string.Format("{0} - {1}", newT.ExerciseStart.HasValue ? newT.ExerciseStart.Value.ToString(UserContext.Current.DateFormat) : "Open", newT.ExerciseEnd.HasValue ? newT.ExerciseEnd.Value.ToString(UserContext.Current.DateFormat) : "Open"),
                                    Changed = true
                                },
                                new VMLeaseAccountingReviewItem {
                                    Label = "Duration",
                                    Detail = string.Format("{0:" + UserContext.Current.DateFormat + "} - {1}", newT.TermStart, newT.TermEnd.HasValue ? newT.TermEnd.Value.ToString(UserContext.Current.DateFormat) : "Open"),
                                    Changed = true
                                },
                                new VMLeaseAccountingReviewItem {
                                    Label = "Status",
                                    Detail = newT.State,
                                    Changed = true
                                }
                            },
                                TermName = newT.TermName
                            };
                            if (newT.ExerciseStart == null && newT.ExerciseEnd == null)
                            {
                                o.NewItems.RemoveAt(0);
                            }
                            return o;

                        case LeaseAccountingReviewContractChangeListModel.ChangeTypes.Updated:
                            string change;
                            if (newT.TermEnd != oldT.TermEnd)
                            {
                                change = "Duration Edited";
                            }
                            else
                            {
                                change = newT.State == "Exercised" ? "Exercised" : oldT.TermState == "Exercised" ? "Un-Exercised" : "State Changed";
                            }
                            VMLeaseAccountingReviewTermChange result = new VMLeaseAccountingReviewTermChange
                            {
                                Change = change,
                                Duration = string.Format("{0:" + UserContext.Current.DateFormat + "} - {1}", newT.TermStart, newT.TermEnd.HasValue ? newT.TermEnd.Value.ToString(UserContext.Current.DateFormat) : "Open"),
                                PreviousItems = new List<VMLeaseAccountingReviewItem> {
                                    new VMLeaseAccountingReviewItem {
                                        Label = "Exercise Window",
                                        Detail = string.Format("{0} - {1}", oldT.ExerciseStart.HasValue ? oldT.ExerciseStart.Value.ToString(UserContext.Current.DateFormat) : "Open", oldT.ExerciseEnd.HasValue ? oldT.ExerciseEnd.Value.ToString(UserContext.Current.DateFormat) : "Open"),
                                        Changed = newT.ExerciseStart != oldT.ExerciseStart || newT.ExerciseEnd != oldT.ExerciseEnd
                                    },
                                    new VMLeaseAccountingReviewItem {
                                        Label = "Duration",
                                        Detail = string.Format("{0:" + UserContext.Current.DateFormat + "} - {1}", oldT.TermStart, oldT.TermEnd == null ? "Open" : oldT.TermEnd.Value.ToString(UserContext.Current.DateFormat)),
                                        Changed = newT.TermEnd != oldT.TermEnd
                                    },
                                    new VMLeaseAccountingReviewItem {
                                        Label = "Status",
                                        Detail = FormatExerciseState(oldT.ExerciseState),
                                        Changed = newT.State != oldT.TermState
                                    }
                                },
                                NewItems = new List<VMLeaseAccountingReviewItem> {
                                    new VMLeaseAccountingReviewItem {
                                        Label = "Exercise Window",
                                        Detail = string.Format("{0} - {1}", newT.ExerciseStart.HasValue ? newT.ExerciseStart.Value.ToString(UserContext.Current.DateFormat) : "Open", newT.ExerciseEnd.HasValue ? newT.ExerciseEnd.Value.ToString(UserContext.Current.DateFormat) : "Open"),
                                        Changed = newT.ExerciseStart != oldT.ExerciseStart || newT.ExerciseEnd != oldT.ExerciseEnd
                                    },
                                    new VMLeaseAccountingReviewItem {
                                        Label = "Duration",
                                        Detail = string.Format("{0:" + UserContext.Current.DateFormat + "} - {1}", newT.TermStart, newT.TermEnd.HasValue ? newT.TermEnd.Value.ToString(UserContext.Current.DateFormat) : "Open"),
                                        Changed = newT.TermEnd != oldT.TermEnd
                                    },
                                    new VMLeaseAccountingReviewItem {
                                        Label = "Status",
                                        Detail = newT.State,
                                        Changed = newT.State != oldT.TermState
                                    }
                                },
                                TermName = newT.TermName
                            };
                            if (oldT.ExerciseStart == null && oldT.ExerciseEnd == null)
                            {
                                result.PreviousItems.RemoveAt(0);
                            }

                            if (newT.ExerciseStart == null && newT.ExerciseEnd == null)
                            {
                                result.NewItems.RemoveAt(0);
                            }
                            return result;

                        case LeaseAccountingReviewContractChangeListModel.ChangeTypes.Removed:
                            VMLeaseAccountingReviewTermChange o2 = new VMLeaseAccountingReviewTermChange
                            {
                                Change = "Removed",
                                Duration = string.Format("{0:" + UserContext.Current.DateFormat + "} - {1}", oldT.TermStart, oldT.TermEnd == null ? "Open" : oldT.TermEnd.Value.ToString(UserContext.Current.DateFormat)),
                                PreviousItems = new List<VMLeaseAccountingReviewItem> {
                                new VMLeaseAccountingReviewItem {
                                    Label = "Exercise Window",
                                    Detail = string.Format("{0} - {1}", oldT.ExerciseStart.HasValue ? oldT.ExerciseStart.Value.ToString(UserContext.Current.DateFormat) : "Open", oldT.ExerciseEnd.HasValue ? oldT.ExerciseEnd.Value.ToString(UserContext.Current.DateFormat) : "Open"),
                                    Changed = true
                                },
                                new VMLeaseAccountingReviewItem {
                                    Label = "Duration",
                                    Detail = string.Format("{0:" + UserContext.Current.DateFormat + "} - {1}", oldT.TermStart, oldT.TermEnd == null ? "Open" : oldT.TermEnd.Value.ToString(UserContext.Current.DateFormat)),
                                    Changed = true
                                },
                                new VMLeaseAccountingReviewItem {
                                    Label = "Status",
                                    Detail = FormatExerciseState(oldT.ExerciseState),
                                    Changed = true
                                }
                            },
                                NewItems = new List<VMLeaseAccountingReviewItem> {
                                new VMLeaseAccountingReviewItem {
                                    Label = "NA",
                                    Detail = "",
                                    Changed = true
                                }
                            },
                                TermName = oldT.TermName
                            };
                            if (oldT.ExerciseStart == null && oldT.ExerciseEnd == null)
                            {
                                o2.PreviousItems.RemoveAt(0);
                            }
                            return o2;

                        default:
                            throw new ArgumentException("Unknown change type!");
                    }
                }).ToList();
        }

        /// <summary>
        /// The GetParentContractROUTotals.
        /// </summary>
        /// <param name="parentContract">The parentContract<see cref="AgreedValueContractEditModel"/>.</param>
        /// <param name="ignoreSubContract">The ignoreSubContract<see cref="int?"/>.</param>
        /// <returns>The <see cref="Dictionary{int, Decimal}"/>.</returns>
        private static Dictionary<int, decimal> GetParentContractROUTotals(AgreedValueContractEditModel parentContract, int? ignoreSubContract)
        {
            Dictionary<int, decimal> totals = parentContract.Reviews
                .Where(r => r.ActionedReview != null)
                .SelectMany(r => r.ActionedReview.Costs.Select(c => c.AssetID))
                .Distinct()
                .ToDictionary(a => a, a => 0M);
            //Add the old stuff
            parentContract.SubContracts.Where(sc => sc.ContractID != ignoreSubContract && !sc.ContractIsArchived).ToList()
                .ForEach(sc =>
                {
                    if (totals.ContainsKey(sc.AssetID))
                    {
                        totals[sc.AssetID] += sc.Percentage;
                    }
                    else
                    {
                        totals[sc.Asset.ParentID.Value] += sc.Percentage;
                    }
                });
            return totals;
        }

        /// <summary>
        /// The LimitedEditAVReview.
        /// </summary>
        /// <param name="review">The review<see cref="VMAgreedValueReviewEditModel"/>.</param>
        /// <param name="currencyID">The currencyID<see cref="int"/>.</param>
        /// <returns>The <see cref="ExtendedJsonResult"/>.</returns>
        private ExtendedJsonResult LimitedEditAVReview(VMAgreedValueReviewEditModel review, int currencyID)
        {
            ViewBag.CurrencyFormat = localeService.GetCurrency(currencyID).FormatString;
            return ExtendedJson(new
            {
                success = true,
                html = RenderVariantPartialViewToString("DisplayTemplates/VMAgreedValueReviewEditModel", review),
                rows = review.Costs
            });
        }

        /// <summary>
        /// The SaveAssetScheduleItemDetails.
        /// </summary>
        /// <param name="avcontract">The avcontract<see cref="VMAgreedValueContractEditModel"/>.</param>
        private void SaveAssetScheduleItemDetails(VMAgreedValueContractEditModel avcontract)
        {
            if (avcontract.AssetSchedule.Count > 0)
            {
                assetService.UpdateLeaseAccountingAssetDetails(avcontract.AssetSchedule.Select(c =>
                new LeaseAccountingAssetDetails
                {
                    AssetID = c.AssetID,
                    AvailableForUseDate = c.AvailableForUseDate,
                    CostCenter = c.CostCenter,
                    DepreciationStartDate = c.DepreciationStartDate,
                    GLCode = c.GLCode,
                    UnitPrice = c.UnitPrice,
                    AssetOwner = c.AssetOwner,
                    AssetOwnerID = c.AssetOwnerID,
                    AssetUser = c.AssetUser,
                    AssetUserID = c.AssetUserID,
                    BusinessUnit = c.BusinessUnit,
                    BusinessUnitID = c.BusinessUnitID,
                    LegalEntity = c.LegalEntity,
                    LegalEntityID = c.LegalEntityID
                }).ToList());
            }
        }

        /// <summary>
        /// The SetupEditViewBag.
        /// </summary>
        /// <param name="model">The model<see cref="VMContractEditModel"/>.</param>
        private void SetupEditViewBag(VMContractEditModel model)
        {
            List<CurrencyViewModel> currencies = localeService.GetAllCurrencies().ToList();
            CurrencyViewModel currency = currencies.Find(c => c.CurrencyID == model.CurrencyID) ?? currencies[0];
            ViewBag.CurrencyFormat = currency.FormatString;

            ViewBag.GuaranteeTypes = contractService.GetGuaranteeTypes();
            ViewBag.Categories = costCategoryService.GetCostCategorySelectList().Select(s => new SelectListItem { Text = s.Name, Value = s.Key }).ToList();
            ViewBag.CPIRegions = contractService.GetCPIRegionList();
            IList<string> status = contractService.GetStatusList();
            status.Add(model.Status);
            ViewBag.Statuses = status.Distinct().OrderBy(s => s).Select(s => new SelectListItem { Text = s, Value = s }).ToList();
            model.VendorName = contactService.GetContactDisplayName(model.VendorID);
            ViewBag.ContractedPartyName = model.ContractedPartyID.HasValue ? contactService.GetContactDisplayName(model.ContractedPartyID.Value) : "";
            List<ContractTypeEditModel> types = contractTypeService.GetContractTypes();
            ViewBag.ContractTypes = types;
            ContractTypeEditModel contractType = types.FirstOrDefault(t => t.ContractTypeID == model.ContractTypeID);
            if (contractType != null)
            {
                model.ContractType = contractType.Name;
                model.ContractCategory = contractType.Category;
            }
            ViewBag.Currencies = currencies.Select(c => new SelectListItem { Text = c.Name, Value = c.CurrencyID.ToString() }).ToList();
            ViewBag.Metrics = contractService.GetAllInUseMetricTypes();
            ViewBag.AssetID = ContextAssetID;
            //TODO
            //ViewBag.AvailableAssets = assetService.GetAssetSelectList(currency.CurrencyID);
            //ViewBag.Jurisdictions = _locale.GetTaxJurisdictions().ToList();
            ViewBag.StartDate = model.Terms.OrderBy(t => t.TermStart).First().TermStart;
            ViewBag.InvoiceTypes = invoiceTypeService.GetInvoiceTypes().Select(g => new SelectListItem { Text = g.Name, Value = g.InvoiceTypeID.ToString() }).ToList();
            ViewBag.LeaseAccountingAccountCodes = leaseAccountingService.GetAccountCodeSegments();
            ViewBag.LeaseAccountingHiddenFields = ClientContext.Current.GetConfigurationSetting("LeaseAccounting.Fields.Hidden", "").Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).ToArray();
            ViewBag.LeaseAccountingLeaseTypes = contractService.GetLeaseTypes().ToList();
            ViewBag.AssetCategoryTypes = new List<string>
            {
                "Office Administration", "Operational"
            };
            ViewBag.LeaseAccountingLedgerSystems = leaseAccountingService.GetLedgerSystems().Select(s => new SelectListItem { Value = s.Key, Text = s.Value }).ToList();
            if (model is VMSubContractEditModel)
            {
                VMSubContractEditModel subcontractmodel = model as VMSubContractEditModel;

                List<AssetListModel> assetlists = assetService.FindMatchingAssets("", null, status: assetService.GetAssetStatuses().ToArray());
                Dictionary<int, AssetListModel> AssetDictionary = assetlists.ToDictionary(a => a.AssetID, a => a);
                Dictionary<int, string> AssetNameDictionary = new Dictionary<int, string>();

                if (subcontractmodel.ParentContracts.Count > 0)
                {
                    VMAgreedValueReviewEditModel lastActioned = subcontractmodel.Reviews.Where(r => r.ActionedReviewID.HasValue).OrderBy(r => r.ReviewDate).LastOrDefault();

                    //Validation failed
                    AgreedValueContractViewModel ParentContract = contractService.GetContractView(subcontractmodel.ParentContracts[0].SubContractMappings[0].ParentContractID.Value) as AgreedValueContractViewModel;
                    IEnumerable<int> assetids = subcontractmodel.ParentContracts.SelectMany(pc =>
                             pc.SubContractMappings.Select(sm => sm.AssetID))
                             .Union(subcontractmodel.ParentContracts.SelectMany(pc =>
                             pc.SubContractMappings.Select(sm => sm.ParentAssetID)));
                    IEnumerable<AssetListModel> Assets = assetlists.Where(a => assetids.Contains(a.AssetID));
                    IEnumerable<int?> parents = Assets.Select(a => a.ParentID).Where(p => p.HasValue).Distinct();
                    Dictionary<int, AssetViewModel> childAssets = assetlists
                        .Where(a => parents.Contains(a.AssetID) || assetids.Contains(a.AssetID))
                        .Select(a => SimpleMapper.Map<AssetListModel, AssetViewModel>(a))
                        .ToDictionary(a => a.AssetID);
                    subcontractmodel.ParentContracts.ForEach(sm =>
                    {
                        sm.SubContractMappings.ForEach(sm2 =>
                        {
                            //if ParentAssetID is set use it
                            sm2.ParentAssetID = (sm2.ParentAssetID > 0) ? sm2.ParentAssetID :
                            //Check the Asset that the mapping is for and get it's ParentID if it has it
                                (childAssets[sm2.AssetID].ParentID ?? sm2.AssetID);
                            sm2.ParentAsset = childAssets[sm2.ParentAssetID];
                            sm2.ParentContract = ParentContract;
                            sm2.Asset = (sm2.SubContractOptions == VMSubContractMappingModel.SubContractAssetOptions.CreateNewAsset) ? null : childAssets[sm2.AssetID];
                            sm2.SubContractOptions = sm2.AssetID > 0 ? VMSubContractMappingModel.SubContractAssetOptions.UseExistingChild : VMSubContractMappingModel.SubContractAssetOptions.CreateNewAsset;
                            sm2.ExistingChildAssets = childAssets.Values
                                .Where(a => a.AssetID == sm2.ParentAssetID
                                || (a.ParentID.HasValue && a.ParentID == sm2.ParentAssetID)).ToList();
                        });
                    });

                    subcontractmodel.ParentContracts
                        .SelectMany(a => a.SubContractMappings)
                        .Where(scm => scm.SubContractOptions == VMSubContractMappingModel.SubContractAssetOptions.CreateNewAsset).ToList()
                        .ForEach(sm =>
                        {
                            AssetNameDictionary.Add(sm.ChildAssetDetails.ID, sm.ChildAssetDetails.Name);
                        });
                }
                ViewBag.Assets = AssetNameDictionary.Select(a => a.Value).ToArray();
            }
            else if (model is VMAgreedValueContractEditModel editModel)
            {
                VMAgreedValueContractEditModel avc = editModel;
                VMAgreedValueReviewEditModel lastActioned = avc.Reviews.Where(r => r.ActionedReviewID.HasValue).OrderBy(r => r.ReviewDate).LastOrDefault();
                //TODO: this may have speedup by loading all relevant assets GetAssetView once, rather than per-cost (and even by just projecting only the FullNames)
                string[] assetlist = lastActioned != null ?
                    lastActioned.ActionedReview.ActionedCosts_NotInvoiced.Select(a => assetService.GetAssetView(a.AssetID, false, false, false, false, false).FullName).OrderBy(a => a).ToArray()
                    : new[] { assetService.GetAssetView(ContextAssetID, false, false, false, false, false).FullName };

                ViewBag.Assets = assetlist;
            }
            else
            {
                VMRateContractEditModel rvc = (VMRateContractEditModel)model;
                VMRateReviewEditModel lastActioned = rvc.Reviews.Where(r => r.ActionedReview != null).OrderBy(r => r.ReviewDate).LastOrDefault();
                ViewBag.Assets = lastActioned != null ? lastActioned.ActionedReview.Assets.Select(a => assetService.GetAssetView(a, false, false, false, false, false).FullName).OrderBy(a => a).ToArray() : new[] { assetService.GetAssetView(ContextAssetID, false, false, false, false, false).FullName };
            }
            ViewBag.InvoicesTypes = invoiceTypeService.GetInvoiceTypes().ToList();
            model.OtherClauses.ForEach(c =>
            {
                ContractTypeClauseEditModel clause = contractType.PredefinedClauses.FirstOrDefault(c2 => c2.Category == c.Category && c2.Clause == c.Clause);
                c.IsPredefinedClause = clause != null;
                c.IsRequired = clause != null && c.IsRequired;
                c.PercentageFieldMode = clause == null ? FieldMode.Optional : clause.PredefinedClause.PercentageFieldMode;
                c.YearFieldMode = clause == null ? FieldMode.Optional : clause.PredefinedClause.YearFieldMode;
                c.AreaFieldMode = clause == null ? FieldMode.Optional : clause.PredefinedClause.AreaFieldMode;
                c.AmountPayableMode = clause == null ? FieldMode.Optional : clause.PredefinedClause.AmountPayableMode;
                c.PayableToMode = clause == null ? FieldMode.Optional : clause.PredefinedClause.PayableToMode;
                c.AmountReceivableMode = clause == null ? FieldMode.Optional : clause.PredefinedClause.AmountReceivableMode;
                c.ReceivableFromMode = clause == null ? FieldMode.Optional : clause.PredefinedClause.ReceivableFromMode;
            });
        }
        public ActionResult AttachFileToDeal(int ID)
        {
            var client = ServiceLocator.Current.GetInstance<ILeaseAcceleratorClient>();
            var service = new LeaseAcceleratorSynchronisationService(client);
            var review = leaseAccountingService.GetLeaseAccountingReviewEdit(ID);
            var contract = contractService.GetContractView(review.ContractID);
            var filemodel = fileService.GetEntitySpecialFile(contract.EntityID, "LeaseNormalization");
            if (filemodel != null)
            {
                var fileblob = fileService.GetFileBlob(filemodel);
                service.AttachFileToDeal(ID, fileblob);
            }
            return new EmptyResult();
        }

        /// <summary>
        /// Create a PDF for Normalization - gets the cost instances and groups them in different ways. In "before" the costs don't have their costs normalized. In "after" the costs are all scooped up to a payment date if they occur in that month
        /// This attaches the file to the contract with a RoleKey = "LeaseNormalization" with a timestamp in the file name.
        /// </summary>
        /// <param name="ID">Lease accounting Review ID</param>
        /// <param name="generatePDF">Flag to toggle PDF generation</param>
        public ActionResult GenerateNormalizationSummary(int ID, bool generatePDF = true)
        {

            var review = leaseAccountingService.GetLeaseAccountingReviewEdit(ID);
            var contract = contractService.GetContractView(review.ContractID);
            //121
            var paidInArrears = review.CurrentReview().ActionedReview.Costs.First().PaidInArrears;
            var model = new LeaseNormalizationData
            {
                Review = review,
                CurrencyFormat = contract.CurrencyFormat,
                Before = LeaseAccountingProviderFactory.Current.GetPreNormalizationData(review).Where(r => r.Amount != 0),
                After = LeaseAccountingProviderFactory.Current.GetPostNormalizationData(review).Where(r => r.Amount != 0),
                Rows = LeaseAccountingReviewExtensions.GetLeaseAccountingReviewRentalRowsForContract(review, true, true, true, false).ToList(),
                //before startdate but not in the same month
                InterimRentAmount = LeaseAccountingProviderFactory.Current.CalculateInterimRent(review),
                InArrears = paidInArrears
            };


            var termend = review.ProjectTermEnd();
            var ThisAnniEnd = FitDate(termend.Year, termend.Month, review.LeaseAccounting_StartDate.Day);
            var NextAnniEnd = FitDate(termend.Year, termend.Month + 1, review.LeaseAccounting_StartDate.Day);
            DateTime FitDate(int year, int month, int day)
            {
                while (month > 12) { month -= 12; year++; }
                return new DateTime(year, month, Math.Min(DateTime.DaysInMonth(year, month), day));
            }
            if (ThisAnniEnd < termend)
            {
                model.ProjectedNormalizedEndDate = NextAnniEnd.AddDays(-1);
            }
            else
            {
                model.ProjectedNormalizedEndDate = ThisAnniEnd.AddDays(-1);
            }
            if (generatePDF)
            {
                if (XSettings.InstallLicense("X/VKS0cNn5FhpydaGfTQKt+0efQWCtVwkfTQwuG8Xh9klgnCfSW7KpFWQ0lkwg8KCtU34j9HuSERr6IiQbd4xFMhfGGVB3M/3TFMO/XgBjbi1y7S5MlUFrjUWBKMcmImUL1oUMFb8wtwCFVMoSiSIEERXiebQ2W5r8l4z1spFM/G3rsp8hHg4WTXrL0o4wVRZkwX2VEW83TPKiUtWZBusSRG+WPNBtZycrM="))
                {

                    //Convert HTML to PDF
                    Doc theDoc = new Doc();

                    theDoc.HtmlOptions.Engine = EngineType.Gecko;
                    theDoc.HtmlOptions.ForGecko.UseScript = true;
                    theDoc.HtmlOptions.ForGecko.Media = MediaType.Screen;
                    theDoc.HtmlOptions.PageLoadMethod = PageLoadMethodType.WebBrowserNavigate;
                    theDoc.HtmlOptions.HostWebBrowser = true;
                    theDoc.HtmlOptions.DeactivateWebBrowser = false;

                    theDoc.MediaBox.String = "A4";
                    theDoc.Rect.String = "A4";
                    theDoc.Rect.Width = 8.27 * 72 - 20;//getPointSize(8.27, resolution); // 8.27 inches wide
                    theDoc.Rect.Height = 11.69 * 72 - 20; // 11.69 inches long
                    theDoc.MediaBox.String = "10 10 " + theDoc.Rect.Width + " " + theDoc.Rect.Height;
                    theDoc.Rect.Inset(20, 20);

                    string htmlFile = base.RenderVariantPartialViewToString("Template/NormalizationTemplate", model);
                    var theID = theDoc.AddImageHtml(htmlFile);
                    //now accommodate mult pages
                    while (theDoc.Chainable(theID))
                    {
                        theDoc.Page = theDoc.AddPage();
                        theID = theDoc.AddImageToChain(theID);
                    }

                    // Flatten each page
                    for (int i = 1; i <= theDoc.PageCount; i++)
                    {
                        theDoc.PageNumber = i;

                        theDoc.FrameRect();
                        // Add a page count to each page
                        //theDoc.Flatten();
                    }
                    theDoc.FrameRect();

                    //add footer
                    for (int i = 1; i <= theDoc.PageCount; i++)
                    {
                        theDoc.PageNumber = i;

                        int font_sanserif = theDoc.AddFont((XFont.FindFamily("sans-serif").FirstOrDefault() ?? XFont.FindFamily("Arial").First()).Name);
                        theDoc.TextStyle.HPos = 1.0;
                        theDoc.TextStyle.VPos = 0.5;
                        theDoc.Color.String = "128 128 128";
                        theDoc.FontSize = 6;
                        theDoc.Font = font_sanserif;
                        // add footer with date generated this will appear on the bottom left
                        theDoc.Rect.Left = theDoc.MediaBox.Bottom + 12;
                        theDoc.Rect.Top = theDoc.MediaBox.Left + 26;
                        theDoc.Rect.Right = theDoc.MediaBox.Bottom + 400;
                        theDoc.Rect.Bottom = theDoc.MediaBox.Left + 12;
                        theDoc.TextStyle.HPos = 0.0;
                        theDoc.PageNumber = i;
                        theDoc.AddText("Generated: " + DateTime.Now.ToString(UserContext.Current.DateTimeFormat) + ", dates are in the format " + UserContext.Current.DateFormat);

                        //Move the rectangle that the image will sit in
                        //Left and right bounds define the width of the rectangle
                        theDoc.Rect.Left = (theDoc.MediaBox.Width / 2) - 85;
                        theDoc.Rect.Right = (theDoc.MediaBox.Width / 2) + 100;
                        theDoc.Rect.Top = theDoc.MediaBox.Bottom + 10;
                        theDoc.Rect.Bottom = theDoc.MediaBox.Bottom + 33;
                        XImage clientLogoImg = new XImage();
                        clientLogoImg.SetFile(Server.MapPath("~/Content/Images/Logo.png"));
                        theDoc.AddImageObject(clientLogoImg, true);
                    }

                    var fileid = Guid.NewGuid().ToString();
                    theDoc.Save(Path.GetTempPath() + fileid + ".pdf");
                    //if (fileService.GetEntitySpecialFile(contract.EntityID, "LeaseNormalization") != null)
                    //{
                    //    fileService.RemoveEntitySpecialFile(contract.EntityID, "LeaseNormalization");
                    //}
                    try
                    {
                        fileService.SaveFile(
                        new FileEditModel
                        {
                            EntityID = contract.EntityID,
                            Description = "Lease Normalization",
                            RoleKey = "LeaseNormalization",
                            FileExtension = ".pdf",
                            FileName = $"Lease Normalization " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf",
                            FileID = Guid.NewGuid(),
                            UploadDate = DateTime.Now
                        },
                        System.IO.File.ReadAllBytes(Path.GetTempPath() + fileid + ".pdf"), true);
                    }
                    catch (DomainSecurityException dsexc)
                    {
                        return ExtendedJson(new { success = false, message = "An Error occurred - " + dsexc.Message });
                    }
                    //return File(System.IO.File.ReadAllBytes(Path.GetTempPath() + fileid + ".pdf"), "application/pdf", "Lease Normalization.pdf");
                }
            }
            return View("Template/NormalizationTemplate", model);
        }

        private List<SelectListItem> GetValidModificationDates(AgreedValueContractEditModel contract, List<LeaseAccountingReviewEditModel> leaseAccountingReviews)
        {
            var paymentPattern = contract.CurrentReview().ActionedReview.Costs.First().PaymentPattern;
            var paymentFrequency = contract.CurrentReview().ActionedReview.Costs.First().PaymentFrequency;

            var contractStartDate = contract.LeaseAccounting_StartDate;
            var firstDayOfPaymentPeriod = leaseAccountingReviews.First().LeaseAccounting_StartDate;
            var latestContractModificationDate = GetLatestSynchronizationModificationDate(leaseAccountingReviews);
            var contractExpiryDate = contract.CurrentEndingTerm().TermEnd;
            var terminationDate = contract.TerminationDate;

            var possibleDates = contractService.GetValidLeaseVariationDates(contractStartDate, firstDayOfPaymentPeriod, contractExpiryDate, terminationDate, latestContractModificationDate, paymentPattern, paymentFrequency);

            List<SelectListItem> selectListItems = new List<SelectListItem>();
            var groupedDates = possibleDates.OrderByDescending(dates => dates).GroupBy(d => d.Year);
            foreach (var group in groupedDates)
            {
                SelectListGroup selectListGroup = new SelectListGroup { Name = group.Key.ToString() };
                foreach (var date in group)
                {
                    SelectListItem selectListItem = new SelectListItem
                    {
                        Text = date.ToString(UserContext.Current.DateFormat),
                        Value = date.ToString(UserContext.Current.DateFormat),
                        Group = selectListGroup
                    };
                    selectListItems.Add(selectListItem);
                }
            }
            return selectListItems;
        }

        public static DateTime? GetLatestSynchronizationModificationDate(List<LeaseAccountingReviewEditModel> leaseAccountingReviews)
        {
            //LeaseAccountingReviews is already in descending order by GetPriorLeaseAccountingReviews
            foreach (var review in leaseAccountingReviews)
            {
                if (review.SynchronisationActions.Any(syncList => syncList.Action == LeaseAcceleratorAPI.Constants.SynchronisationActionMethods.ModifyDeal)
                    && review.State == "Synchronized"
                    && review.LeaseAccounting_VaryLeaseEffectiveDate != null)
                {
                    return review.LeaseAccounting_VaryLeaseEffectiveDate;
                }
            }
            return null;
        }

        public string GetLeaseAccountingReviewDeclineNotes(int ID)
        {
            return leaseAccountingService.GetLeaseAccountingReviewDeclineNoteByLeaseAccountingReviewID(ID);
        }
        /// <summary>
        /// Defines the <see cref="CostTemplateMapping" />.
        /// </summary>
        private class CostTemplateMapping
        {
            /// <summary>
            /// Gets or sets the Cost.
            /// </summary>
            public VMAgreedValueContractCostEditModel Cost { get; set; }

            /// <summary>
            /// Gets or sets the Template.
            /// </summary>
            public VMActionAVReviewModel.VMActionAVReviewTemplateModel Template { get; set; }
        }

        /// <summary>
        /// Defines the <see cref="SearchContractResult" />.
        /// </summary>
        private class SearchContractResult
        {
            /// <summary>
            /// Gets or sets the Assets.
            /// </summary>
            [JsonProperty(PropertyName = "assets")]
            public string[] Assets { get; set; }

            /// <summary>
            /// Gets or sets the Id.
            /// </summary>
            [JsonProperty(PropertyName = "id")]
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the Label.
            /// </summary>
            [JsonProperty(PropertyName = "label")]
            public string Label { get; set; }

            /// <summary>
            /// Gets or sets the Type.
            /// </summary>
            [JsonProperty(PropertyName = "type")]
            public string Type { get; set; }

            /// <summary>
            /// Gets or sets the VendorId.
            /// </summary>
            [JsonProperty(PropertyName = "vendorid")]
            public int VendorId { get; set; }

            /// <summary>
            /// Gets or sets the VendorName.
            /// </summary>
            [JsonProperty(PropertyName = "vendorname")]
            public string VendorName { get; set; }
        }

        [HttpGet]
        public ActionResult IsEntityAndContractCurrencyDifferent(int assetID, int currencyID)
        {
            bool result = contractService.IsEntityAndContractCurrencyDifferent(assetID, currencyID);
            return ExtendedJson(new { success = result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Validate if any paid invoice exists for the contract review for Vary Contract
        /// </summary>
        /// <param name="id">The contract id</param>
        /// <returns></returns>
        [HttpPost]
        public ExtendedJsonResult ValidatePaidInvoiceForVaryContractReview(int id, VaryLeaseContractEditModel model = null)
        {
            return ValidatePaidInvoice(id, model);
        }

        /// <summary>
        /// Validate if any paid invoice exists for the contract review for Edit Contract
        /// </summary>
        /// <param name="id">The contract id</param>
        /// <returns></returns>
        [HttpPost]
        public ExtendedJsonResult ValidatePaidInvoiceForEditContractReview(int id, VMAgreedValueContractEditModel model = null)
        {
            return ValidatePaidInvoice(id, model);
        }

        private ExtendedJsonResult ValidatePaidInvoice(int id, dynamic model = null)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return JsonUnauthorized();
            }

            var paidInvoices = invoiceService.GetInvoiceCostListByContractID(id)
                                             .Where(x => x.IsPaid)
                                             .ToList();
            if (!paidInvoices.Any())
            {
                return ExtendedJson(new
                {
                    success = false,
                    message = "There are no paid invoice for the contract review",
                    invoices = new List<InvoiceEditModel>()
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                AgreedValueContractEditModel contract = contractService.GetContractEdit(id, false) as AgreedValueContractEditModel;
                if (contract == null)
                {
                    return ExtendedJson(new
                    {
                        success = false,
                        message = "The contract you're trying to modify could not be found and may have been removed by another user."
                    }, JsonRequestBehavior.AllowGet);
                }
                if (contract.IsArchived)
                {
                    return ExtendedJson(new
                    {
                        success = false,
                        message = "The contract you're trying to modify is archived and cannot be modified."
                    }, JsonRequestBehavior.AllowGet);
                }
                if (!contract.SubjectToLeaseAccounting)
                {
                    return ExtendedJson(new
                    {
                        success = false,
                        message = "The contract you're trying to modify is not subject to lease accounting."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Get the list of existing reviews
                var existingReviews = contract.Reviews;

                // Get the list of new reviews from the model
                var newReviews = model is VaryLeaseContractEditModel ? ((VaryLeaseContractEditModel)model).Reviews : ((VMAgreedValueContractEditModel)model).Reviews;

                // Find the reviews that have been changed or removed
                var changedOrRemovedReviews = new List<AgreedValueReviewEditModel>();

                foreach (var existingReview in existingReviews)
                {
                    var newReview = newReviews.FirstOrDefault(r => r.ReviewID == existingReview.ReviewID && r.State == existingReview.State);
                    if (newReview == null)
                    {
                        changedOrRemovedReviews.Add(existingReview);
                    }
                }

                List<VMActionAVReviewModel> newActionReview = new List<VMActionAVReviewModel>();
                foreach (var newReview in newReviews)
                {
                    if (newReview.ActionedReview != null && newReview.ActionedReview.ReviewID <= 0)
                    {
                        newActionReview.Add(newReview.ActionedReview);
                    }
                }
                if (newActionReview.Any())
                {
                    foreach (var updatedReview in newActionReview)
                    {
                        var oldUpdatedReview = existingReviews.FirstOrDefault(r => r.ReviewDate == updatedReview.ReviewDate);
                        if (oldUpdatedReview != null && !changedOrRemovedReviews.Any(r => r.ReviewDate == oldUpdatedReview.ReviewDate))
                        {
                            changedOrRemovedReviews.Add(oldUpdatedReview);
                        }
                    }
                }

                var effectedInvoices = new List<InvoiceEditModel>();
                if (changedOrRemovedReviews.Any())
                {
                    var effectedReview = changedOrRemovedReviews.OrderBy(x => x.ReviewDate).FirstOrDefault();
                    if (effectedReview != null)
                    {
                        var invoicesInReviewPeriod = paidInvoices
                           .Where(i => i.DateOfInvoice >= effectedReview.ReviewDate)
                           .ToList();

                        effectedInvoices.AddRange(invoicesInReviewPeriod);
                    }
                }
                if (effectedInvoices.Any())
                {
                    return ExtendedJson(new
                    {
                        success = true,
                        message = "There are paid invoices for the contract review",
                        invoices = effectedInvoices
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return ExtendedJson(new
                    {
                        success = false,
                        message = "There are no paid invoice for the contract review",
                        invoices = new List<InvoiceEditModel>()
                    }, JsonRequestBehavior.AllowGet);
                }
            }
        }

        /// <summary>
        /// Shows user confirmation dialog for Rollback Lease Variation
        /// </summary>
        /// <param name="leaseAccountingReviews"></param>
        /// <returns></returns>
        public PartialViewResult BeginRollbackLeaseVariation(int id)
        {
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit))
            {
                return PartialUnauthorized();
            }

            if (!assetService.AssetIsEditable(ContextAssetID))
            {
                return PartialUnauthorized();
            }

            var contract = contractService.GetContractEdit(id, false) as AgreedValueContractEditModel;

            if (contract == null)
            {
                return PartialView("Partial/Error", new { message = "The contract you're trying to vary could not be found and may have been removed by another user." });
            }
            if (contract.IsArchived)
            {
                return PartialView("Partial/Error", new { message = "The contract you're trying to vary is archived and cannot be modified." });
            }
            if (!contract.SubjectToLeaseAccounting)
            {
                return PartialView("Partial/Error", new { message = "The contract you're trying to vary is not subject to lease accounting." });
            }

            var priorLeaseAccountingReviews = leaseAccountingService.GetPriorLeaseAccountingReviews(id, TimeSpan.MaxValue).ToList();

            if (priorLeaseAccountingReviews.Count <= 0)
            {
                return PartialView("Partial/Error", new { message = "Prior lease accounting review not found." });
            }

            if (priorLeaseAccountingReviews.First().State != LeaseAccountingConstants.LeaseAccountingStates.Synchronized)
            {
                return PartialView("Partial/Error", new { message = "The last lease accounting review has to be Synchronized before rollback." });
            }

            if (!priorLeaseAccountingReviews.First().SynchronisationActions.Any(a => a.Action == SynchronisationActionMethods.ModifyDeal))
            {
                return PartialView("Partial/Error", new { message = "The last lease accounting review does not include any Modify Deal action." });
            }

            var actiongroups = priorLeaseAccountingReviews.First().GetActionsForLeaseAccountingReview();
            return PartialView("Dialog/BeginRollbackLeaseVariation", actiongroups);
        }

        /// <summary>
        /// Rollbacks Synchronized Lease Variation by Updating Contract and Submits Rollback Review
        /// </summary>
        /// <param name="id">Contract Id</param>
        /// <returns>The <see cref="ExtendedJsonResult"/></returns>
        public ActionResult RollbackLeaseVariation(int id)
        {
            SystemContext.AuditLog.AddAuditEntry("Contract", "RollbackLeaseVariation", "Start", $"Rolling back lease variation for contract id {id}");

            if (!UserContext.Current.EvaluateAccess(true, TestAssetIsAccessible, LeaseAccountingReviewPermissions.Delete)) { return JsonUnauthorized(); }
            if (!UserContext.Current.HasPermission(AssetManagementContractsPermissions.Edit)) { return JsonUnauthorized(); }

            ILeaseAccountingService leaseAccountingService = ServiceLocator.Current.GetInstance<ILeaseAccountingService>();
            var priorLeaseAccountingReviews = leaseAccountingService.GetPriorLeaseAccountingReviews(id, TimeSpan.MaxValue).ToList();
            var lastLeaseAccountingReview = priorLeaseAccountingReviews.Where(x => x.State == LeaseAccountingConstants.LeaseAccountingStates.Synchronized).First();
            var rollingBackToReview = priorLeaseAccountingReviews.Where(x => x.State == LeaseAccountingConstants.LeaseAccountingStates.Synchronized).Skip(1).First();

            try
            {
                SystemContext.AuditLog.AddAuditEntry("Contract", "RollbackLeaseVariation", "Process", $"Last LeaseAccountingReview rolledback using RevertContractToLeaseAccountingReview for contract id {id}, review id {lastLeaseAccountingReview.LeaseAccountingReviewID}");
                leaseAccountingService.RevertContractToLeaseAccountingReview(lastLeaseAccountingReview.LeaseAccountingReviewID);
                var contract = contractService.GetContractEdit(id, true) as AgreedValueContractEditModel;

                SystemContext.AuditLog.AddAuditEntry("Contract", "RollbackLeaseVariation", "Process", $"Drafting new Lease Accounting Review for contract id {id}");
                var draftLeaseAccountingReview = leaseAccountingService.GetDraftLeaseAccountingReviewForContract(contract, false, false);
                draftLeaseAccountingReview.ContractReferenceNo = rollingBackToReview.ContractReferenceNo;
                draftLeaseAccountingReview.IsFormalLeaseAccountingReview = true;
                draftLeaseAccountingReview.Comments = "Rollback Lease Variation : " + lastLeaseAccountingReview.Comments;
                draftLeaseAccountingReview.ContractChangesOnSubmit = LeaseAccountingProviderFactory.Current.GetLeaseAccountingReviewChanges(draftLeaseAccountingReview, lastLeaseAccountingReview);
                draftLeaseAccountingReview.DealID = rollingBackToReview.DealID;

                //updateing the exercise state of the terms to the exercise state of the terms in the review we are rolling back to
                foreach (var term in draftLeaseAccountingReview.Terms)
                {
                    var matchingTerm = rollingBackToReview.Terms.FirstOrDefault(x => x.TermName == term.TermName);
                    if (matchingTerm != null)
                    {
                        term.ExerciseState = matchingTerm.ExerciseState;
                    }
                }

                var context = new ValidationContext(draftLeaseAccountingReview);
                context.Items["AllowMultiplePatterns"] = true;
                var errors = LeaseAccountingProviderFactory.Current.ValidateLeaseAccountingReview(draftLeaseAccountingReview, contract, context).Select(e => e.ErrorMessage).ToList();

                SystemContext.AuditLog.AddAuditEntry("Contract", "RollbackLeaseVariation", "Validation", $"Validation errors for rollback lease variation for contract id {id} - {String.Join(" | ", errors)}");
                if (errors.Count == 0)
                {
                    leaseAccountingService.AddLeaseAccountingReview(draftLeaseAccountingReview);
                    leaseAccountingService.SetLeaseAccountingReviewState(draftLeaseAccountingReview, "Submitted", LeaseAccountingReview_ProcessCode.ROLLBACK_LEASE_VARIATION);
                    SystemContext.AuditLog.AddAuditEntry("Contract", "RollbackLeaseVariation", "Success", $"Rolled back lease variation for contract id {id}, review id {lastLeaseAccountingReview.LeaseAccountingReviewID} successful.");

                    return ExtendedJson(new
                    {
                        success = true,
                        message = "Successfully rolled back Lease Variation"
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    SystemContext.AuditLog.AddAuditEntry("Contract", "RollbackLeaseVariation", "Error", $"Rollback lease variation for contract id {id}, review id {lastLeaseAccountingReview.LeaseAccountingReviewID} failed due to validation error - {String.Join(" | ", errors)}.");

                    return ExtendedJson(new
                    {
                        success = false,
                        message = "Failed to submit review for Lease Variation Rollback"
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                SystemContext.AuditLog.AddAuditEntry("Contract", "RollbackLeaseVariation", "Exception", $"Rolled back lease variation for contract id {id} failed with exception {ex.InnerException}");

                return ExtendedJson(new
                {
                    success = false,
                    message = "An error occured trying to rolling back the latest lease variation"
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}