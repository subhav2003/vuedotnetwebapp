<template>
  <div class="connect-container min-h-screen flex flex-col">
    <NavBar />

    <main class="flex-grow">
      <!-- Filter Section -->
      <section class="bg-white shadow-md rounded-lg py-6 px-4 sm:px-8 mb-6">
        <div class="container mx-auto">
          <div class="flex flex-col md:flex-row md:items-end md:space-x-6 space-y-4 md:space-y-0">
            <!-- Accepts Custom Orders -->
            <div class="flex-1">
              <label class="block text-gray-700 font-semibold mb-2">
                Accepts Custom Orders
              </label>
              <select
                  v-model="acceptsCustom"
                  class="w-full p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-brown-400"
              >
                <option value="">Any</option>
                <option :value="true">Yes</option>
                <option :value="false">No</option>
              </select>
            </div>

            <!-- Sort By -->
            <div class="flex-1">
              <label class="block text-gray-700 font-semibold mb-2">
                Sort By
              </label>
              <select
                  v-model="sortOption"
                  class="w-full p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-brown-400"
              >
                <option value="">Default</option>
                <option value="rating_desc">Rating ↓</option>
                <option value="rating_asc">Rating ↑</option>
                <option value="name_asc">Name A → Z</option>
                <option value="name_desc">Name Z → A</option>
              </select>
            </div>

            <!-- Action Buttons -->
            <div class="flex space-x-4">
              <button
                  @click="fetchFilteredArtisans"
                  class="bg-brown-600 text-white px-4 py-2 rounded-lg hover:bg-brown-700 transition"
              >
                Apply
              </button>
              <button
                  @click="resetFilters"
                  class="bg-gray-200 text-gray-700 px-4 py-2 rounded-lg hover:bg-gray-300 transition"
              >
                Reset
              </button>
            </div>
          </div>
        </div>
      </section>

      <!-- Artisan Grid -->
      <section class="artisans bg-white py-10 px-4 sm:px-8">
        <div class="container mx-auto">
          <div v-if="loading" class="flex justify-center items-center h-64">
            <div class="loader ease-linear rounded-full border-4 border-t-4 border-gray-200 h-12 w-12"></div>
          </div>

          <div
              v-else
              class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6"
          >
            <ArtisanCard
                v-for="artisan in artisans"
                :key="artisan.id"
                :artisan="artisan"
                @view-profile="openPortfolio"
            />
          </div>
        </div>
      </section>

      <!-- Portfolio Overlay -->
      <Portfolio
          v-if="showPortfolio"
          :artisanId="selectedArtisanId"
          @close="closePortfolio"
          class="fixed inset-0 z-50"
      >
        <template #flag>
          <FlagButton v-if="isAuthenticated" type="artisan" :id="selectedArtisanId" />
        </template>
      </Portfolio>
    </main>

    <Footer />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import NavBar from '@/components/NavBar.vue';
import Footer from '@/components/Footer.vue';
import ArtisanCard from '@/components/ArtisanCard.vue';
import Portfolio from '@/components/Portfolio.vue';
import FlagButton from '@/components/FlagButton.vue';

const apiBaseUrl = process.env.VUE_APP_API_URL;
const isAuthenticated = !!localStorage.getItem('authToken');

const artisans = ref([]);
const loading = ref(true);

// — New filter state —
const acceptsCustom = ref('');   // '', true, or false
const sortOption    = ref('');   // '', 'rating_desc', 'rating_asc', 'name_asc', 'name_desc'

const showPortfolio     = ref(false);
const selectedArtisanId = ref(null);

const fetchFilteredArtisans = async () => {
  loading.value = true;
  try {
    const params = new URLSearchParams();
    if (acceptsCustom.value !== '') {
      params.append('accept_custom_order', acceptsCustom.value);
    }
    if (sortOption.value) {
      params.append('sort', sortOption.value);
    }

    const res  = await fetch(`${apiBaseUrl}/api/v1/artisan-filter?${params}`);
    const json = await res.json();
    artisans.value = json.data;
  } catch (err) {
    console.error('Failed to fetch filtered artisans:', err);
  } finally {
    loading.value = false;
  }
};

const resetFilters = () => {
  acceptsCustom.value = '';
  sortOption.value    = '';
  fetchFilteredArtisans();
};

const openPortfolio = (id) => {
  selectedArtisanId.value = id;
  showPortfolio.value = true;
  document.body.style.overflow = 'hidden';
};

const closePortfolio = () => {
  selectedArtisanId.value = null;
  showPortfolio.value = false;
  document.body.style.overflow = '';
};

onMounted(fetchFilteredArtisans);
</script>

<style scoped>
.loader {
  border-top-color: #3498db;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
