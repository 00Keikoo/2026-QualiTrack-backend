using Microsoft.EntityFrameworkCore;
using QualiTrack.Data;
using QualiTrack.Models;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var checklists = new List<Checklist>
        {
            // ── ISO 9001 - Warehouse ──────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "ISO 9001 - Warehouse",
                Standard = "ISO9001",
                Department = "Warehouse",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Goods receipt follows established procedure", Description = "Verify that all incoming goods are received according to the documented receiving procedure, including checking purchase orders and delivery notes.", ClauseRef = "ISO9001", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Incoming goods quality inspection conducted", Description = "Confirm that a quality inspection is performed on all incoming goods before being stored, including visual checks and quantity verification.", ClauseRef = "ISO9001", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Goods labeling and identification are clear", Description = "Ensure all stored goods have clear and legible labels including product name, quantity, batch number, and expiry date where applicable.", ClauseRef = "ISO9001", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "FIFO/FEFO principle is applied", Description = "Check that First In First Out (FIFO) or First Expired First Out (FEFO) principles are consistently applied during goods retrieval and dispatch.", ClauseRef = "ISO9001", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Stock records are accurate and up to date", Description = "Verify that stock records in the system match physical inventory and are updated in real time or at regular intervals.", ClauseRef = "ISO9001", OrderIndex = 5 },
                ]
            },

            // ── ISO 14001 - Warehouse ─────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "ISO 14001 - Warehouse",
                Standard = "ISO14001",
                Department = "Warehouse",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Waste is separated according to category", Description = "Confirm that waste is sorted into designated categories such as organic, non-organic, hazardous, and recyclable materials in accordance with environmental policy.", ClauseRef = "ISO14001", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Waste management (cardboard, plastic, etc.) is in place", Description = "Verify that packaging waste such as cardboard boxes and plastic wrapping are collected, sorted, and disposed of or recycled through approved channels.", ClauseRef = "ISO14001", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Hazardous materials are stored safely", Description = "Check that hazardous materials are stored in designated areas with proper ventilation, secondary containment, and clearly marked warning signs.", ClauseRef = "ISO14001", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "No leaks or environmental contamination found", Description = "Inspect the warehouse area for any signs of liquid leaks, spills, or contamination that could impact soil or drainage systems.", ClauseRef = "ISO14001", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Separate waste bins are available", Description = "Ensure that clearly labeled waste bins for different waste types are available and accessible throughout the warehouse area.", ClauseRef = "ISO14001", OrderIndex = 5 },
                ]
            },

            // ── GMP - Warehouse ───────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "GMP - Warehouse",
                Standard = "GMP",
                Department = "Warehouse",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Goods are stored according to product category", Description = "Ensure that all goods are stored in the appropriate area based on product category to prevent picking errors and cross-contamination.", ClauseRef = "GMP", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Warehouse area is clean and tidy", Description = "Inspect the overall cleanliness of the warehouse including floors, storage racks, and loading/unloading areas for dust, dirt, and leftover materials.", ClauseRef = "GMP", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Products are protected from contamination", Description = "Verify that all stored products are protected from potential contamination sources such as pests, excessive humidity, and exposure to chemical substances.", ClauseRef = "GMP", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "Expired products are clearly segregated", Description = "Confirm that products past their expiry or shelf-life date are clearly separated from sellable stock and marked with appropriate labels.", ClauseRef = "GMP", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Storage pallets are in good condition", Description = "Check the physical condition of all pallets in use to ensure none are damaged, broken, or likely to cause workplace accidents or product damage.", ClauseRef = "GMP", OrderIndex = 5 },
                ]
            },

            // ── ISO 9001 - Production ─────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "ISO 9001 - Production",
                Standard = "ISO9001",
                Department = "Production",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Production SOP is available and followed", Description = "Verify that Standard Operating Procedures for all production activities are documented, accessible to operators, and consistently followed on the production floor.", ClauseRef = "ISO9001", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Machines are in proper working condition", Description = "Confirm that all production machines have undergone scheduled maintenance and are operating within acceptable parameters without defects or safety hazards.", ClauseRef = "ISO9001", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Quality control is performed during production", Description = "Check that in-process quality control checks are conducted at defined intervals and results are recorded in the production log.", ClauseRef = "ISO9001", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "Defective products are recorded and handled", Description = "Ensure that any defective or non-conforming products are identified, tagged, segregated from good products, and reported through the non-conformance process.", ClauseRef = "ISO9001", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Production documentation is complete", Description = "Verify that all required production records including batch records, work orders, and inspection reports are completed accurately and filed properly.", ClauseRef = "ISO9001", OrderIndex = 5 },
                ]
            },

            // ── ISO 14001 - Production ────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "ISO 14001 - Production",
                Standard = "ISO14001",
                Department = "Production",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Production waste is properly managed", Description = "Confirm that all waste generated during production is collected, categorized, and disposed of according to the environmental waste management plan.", ClauseRef = "ISO14001", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Emissions (smoke/gas) are controlled", Description = "Check that air emissions from production processes are within permissible limits and that emission control equipment is functioning properly.", ClauseRef = "ISO14001", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Energy usage is efficient", Description = "Verify that energy consumption is monitored and that energy-saving measures such as turning off idle machines and optimizing production schedules are implemented.", ClauseRef = "ISO14001", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "Wastewater is properly managed", Description = "Ensure that wastewater from production processes is treated before disposal and meets regulatory discharge standards.", ClauseRef = "ISO14001", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Chemical storage is safe and compliant", Description = "Inspect that all chemicals used in production are stored in approved containers, properly labeled, and kept in designated storage areas away from ignition sources.", ClauseRef = "ISO14001", OrderIndex = 5 },
                ]
            },

            // ── GMP - Production ──────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "GMP - Production",
                Standard = "GMP",
                Department = "Production",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Machines are cleaned before use", Description = "Ensure that all production machines are cleaned according to the prescribed procedure before use to prevent contamination of the products being manufactured.", ClauseRef = "GMP", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Products are protected from contamination", Description = "Verify that products throughout the production process are protected from physical, chemical, and biological contamination at all stages.", ClauseRef = "GMP", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Production materials are stored correctly", Description = "Check that all raw materials and production support materials are stored under appropriate conditions including correct temperature, humidity, and cleanliness of the storage area.", ClauseRef = "GMP", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "No food or drinks in the production area", Description = "Confirm that no food, beverages, or personal items are brought into the production area to prevent product contamination.", ClauseRef = "GMP", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Operators are wearing complete PPE", Description = "Ensure that all operators working in the production area are wearing complete and appropriate Personal Protective Equipment (PPE) such as masks, gloves, and work attire.", ClauseRef = "GMP", OrderIndex = 5 },
                ]
            },

            // ── ISO 9001 - QC ─────────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "ISO 9001 - QC",
                Standard = "ISO9001",
                Department = "QC",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Measuring instruments are calibrated regularly", Description = "Verify that all measuring instruments and testing equipment have a documented calibration schedule and have been calibrated accordingly.", ClauseRef = "ISO9001", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Product samples are clearly identified", Description = "Ensure that each product sample taken for testing has a complete identification label including batch number, sampling date, and product code.", ClauseRef = "ISO9001", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Quality check data is traceable", Description = "Confirm that all quality inspection data is fully recorded and traceable back to the production batch and responsible operator.", ClauseRef = "ISO9001", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "Inspection procedures are available and in use", Description = "Check that quality inspection procedures are available in the QC area in their latest version and are actively used by personnel as a reference.", ClauseRef = "ISO9001", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Non-conforming products are properly recorded", Description = "Verify that every product failing to meet specifications is recorded in a non-conformance report detailing the finding, quantity affected, and corrective action taken.", ClauseRef = "ISO9001", OrderIndex = 5 },
                ]
            },

            // ── ISO 14001 - QC ────────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "ISO 14001 - QC",
                Standard = "ISO14001",
                Department = "QC",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Chemical waste is separated by category", Description = "Ensure that chemical waste from testing processes is separated by category such as corrosive, flammable, and toxic waste in accordance with environmental regulations.", ClauseRef = "ISO14001", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Chemical storage is safe and sealed", Description = "Verify that all chemicals are stored in tightly sealed containers, properly labeled, and kept in areas with adequate ventilation.", ClauseRef = "ISO14001", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Spill response procedure is available", Description = "Confirm that a chemical spill response procedure is available in the laboratory area and that personnel understand the steps to be taken in case of a spill.", ClauseRef = "ISO14001", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "Chemical usage is recorded", Description = "Check that all chemical usage is logged in a logbook or recording system capturing chemical type, quantity used, and date of use.", ClauseRef = "ISO14001", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Laboratory area is kept clean", Description = "Ensure the QC laboratory area is always clean and organized, free from spills, unused equipment, and irrelevant materials.", ClauseRef = "ISO14001", OrderIndex = 5 },
                ]
            },

            // ── GMP - QC ──────────────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "GMP - QC",
                Standard = "GMP",
                Department = "QC",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Testing equipment is cleaned after use", Description = "Ensure all testing equipment is cleaned immediately after use according to the established cleaning procedure to prevent cross-contamination between samples.", ClauseRef = "GMP", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Samples are stored in appropriate conditions", Description = "Verify that product samples are stored under appropriate conditions including correct temperature, humidity, and protection from direct light exposure.", ClauseRef = "GMP", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Operators are wearing complete PPE", Description = "Confirm that all QC personnel are wearing complete and appropriate PPE including lab coats, gloves, safety glasses, and masks while conducting tests.", ClauseRef = "GMP", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "No cross-contamination between samples", Description = "Ensure there is no cross-contamination between different product samples by using dedicated equipment, proper labeling, and following sample handling procedures.", ClauseRef = "GMP", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Test results are completely recorded", Description = "Verify that all test results are recorded completely and accurately in the designated forms or system, including test date, operator name, and final result.", ClauseRef = "GMP", OrderIndex = 5 },
                ]
            },

            // ── ISO 9001 - Packaging ──────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "ISO 9001 - Packaging",
                Standard = "ISO9001",
                Department = "Packaging",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Product labels conform to specifications", Description = "Verify that all product labels match the approved specifications including correct product name, ingredients, expiry date format, and regulatory information.", ClauseRef = "ISO9001", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Product packaging is undamaged", Description = "Inspect finished product packaging to ensure there are no tears, dents, leaks, or defects that could compromise product integrity or customer satisfaction.", ClauseRef = "ISO9001", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Released products are inspected before dispatch", Description = "Confirm that all products have passed the release inspection by QC before being dispatched to customers or distribution centers.", ClauseRef = "ISO9001", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "Product barcodes are clearly readable", Description = "Check that barcodes on all packaged products are legible, scannable, and correctly printed without smudging or distortion.", ClauseRef = "ISO9001", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Packaging materials meet required standards", Description = "Verify that all packaging materials used comply with the applicable quality and safety standards including food contact material regulations where applicable.", ClauseRef = "ISO9001", OrderIndex = 5 },
                ]
            },

            // ── ISO 14001 - Packaging ─────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "ISO 14001 - Packaging",
                Standard = "ISO14001",
                Department = "Packaging",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Packaging waste is separated by type", Description = "Ensure that packaging waste such as cardboard, plastic film, and metal is separated into designated waste bins according to material type.", ClauseRef = "ISO14001", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Packaging material usage is monitored", Description = "Verify that the consumption of packaging materials is tracked and recorded to support waste reduction efforts and environmental targets.", ClauseRef = "ISO14001", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Waste bins are clearly labeled", Description = "Confirm that all waste bins in the packaging area are clearly labeled with the type of waste they are intended for to ensure correct waste segregation.", ClauseRef = "ISO14001", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "Plastic usage is minimized", Description = "Check that efforts are in place to minimize the use of single-use plastics in the packaging process, including the use of alternative materials where feasible.", ClauseRef = "ISO14001", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "Packaging waste management procedure is available", Description = "Ensure that a documented procedure for managing packaging waste is available in the area and is understood and followed by packaging personnel.", ClauseRef = "ISO14001", OrderIndex = 5 },
                ]
            },

            // ── GMP - Packaging ───────────────────────────────────────────────
            new()
            {
                Id = Guid.NewGuid(),
                Title = "GMP - Packaging",
                Standard = "GMP",
                Department = "Packaging",
                CreatedAt = DateTime.UtcNow,
                Items =
                [
                    new() { Id = Guid.NewGuid(), Question = "Packaging area is clean and tidy", Description = "Inspect the overall cleanliness of the packaging area including workstations, floors, and equipment to ensure the environment is suitable for product packaging.", ClauseRef = "GMP", OrderIndex = 1 },
                    new() { Id = Guid.NewGuid(), Question = "Products are protected from contamination", Description = "Verify that products during the packaging process are protected from physical, chemical, and biological contamination at all stages.", ClauseRef = "GMP", OrderIndex = 2 },
                    new() { Id = Guid.NewGuid(), Question = "Product labels are correctly applied", Description = "Confirm that all labels are applied correctly on each product including proper alignment, correct information, and no missing or misplaced labels.", ClauseRef = "GMP", OrderIndex = 3 },
                    new() { Id = Guid.NewGuid(), Question = "Packaging materials are stored safely", Description = "Check that all packaging materials are stored in a clean, dry, and secure area to prevent damage, contamination, or unauthorized use.", ClauseRef = "GMP", OrderIndex = 4 },
                    new() { Id = Guid.NewGuid(), Question = "No open products in the packaging area", Description = "Ensure that no unwrapped or open products are left unattended in the packaging area to prevent exposure to contamination or environmental conditions.", ClauseRef = "GMP", OrderIndex = 5 },
                ]
            },
        };



        foreach (var checklist in checklists)
        {
            var exists = await db.Checklists
                .AnyAsync(c => c.Title == checklist.Title && c.Department == checklist.Department);

            if (!exists)
            {
                db.Checklists.Add(checklist);
            }
        }

        await db.SaveChangesAsync();
    }
}