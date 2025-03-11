const CategoryApi = {
    // Get category statistics
    getStatistics: async () => {
        try {
            const response = await fetch('/api/category/statistics');
            if (!response.ok) throw new Error('Failed to fetch statistics');
            return await response.json();
        } catch (error) {
            console.error('Error fetching category statistics:', error);
            throw error;
        }
    },

    // Validate category name
    validateName: async (name, excludeId = null) => {
        try {
            const url = new URL('/api/category/validate-name', window.location.origin);
            url.searchParams.append('name', name);
            if (excludeId) url.searchParams.append('excludeId', excludeId);

            const response = await fetch(url);
            if (!response.ok) throw new Error('Failed to validate name');
            return await response.json();
        } catch (error) {
            console.error('Error validating category name:', error);
            throw error;
        }
    },

    // Search categories
    search: async (term) => {
        try {
            const url = new URL('/api/category/search', window.location.origin);
            if (term) url.searchParams.append('term', term);

            const response = await fetch(url);
            if (!response.ok) throw new Error('Failed to search categories');
            return await response.json();
        } catch (error) {
            console.error('Error searching categories:', error);
            throw error;
        }
    },

    // Get monthly trends
    getMonthlyTrends: async (months = 6) => {
        try {
            const url = new URL('/api/category/monthly-trends', window.location.origin);
            url.searchParams.append('months', months);

            const response = await fetch(url);
            if (!response.ok) throw new Error('Failed to fetch monthly trends');
            return await response.json();
        } catch (error) {
            console.error('Error fetching monthly trends:', error);
            throw error;
        }
    },

    // Batch update categories (Admin only)
    batchUpdate: async (updates) => {
        try {
            const response = await fetch('/api/category/batch-update', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(updates)
            });

            if (!response.ok) throw new Error('Failed to update categories');
            return await response.json();
        } catch (error) {
            console.error('Error updating categories:', error);
            throw error;
        }
    }
};

// Export for use in other modules
window.CategoryApi = CategoryApi;