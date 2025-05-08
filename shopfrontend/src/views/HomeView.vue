<template>
  <div class="home-view flex flex-col min-h-screen bg-gray-900 text-white">
    <!-- Navbar -->
    <NavBar />

    <!-- Toast -->
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Hero Section -->
    <section class="bg-gradient-to-r from-yellow-900 via-yellow-800 to-yellow-900 py-20 text-center">
      <h1 class="text-5xl sm:text-6xl font-black text-yellow-100 mb-4 font-papyrus">
        Welcome to Pustakalaya
      </h1>
      <p class="text-xl sm:text-2xl text-yellow-200 mb-8">
        Explore handpicked books and connect with passionate authors in our vibrant literary marketplace.
      </p>
      <button
          class="inline-block bg-yellow-700 hover:bg-yellow-800 text-white font-semibold py-3 px-8 rounded-lg shadow-lg transition-transform transform hover:-translate-y-1"
      >
        Browse Collection
      </button>
    </section>

    <!-- Books Section -->
    <section class="py-16 w-full">
      <div class="flex flex-col md:flex-row w-full">
        <!-- Left Panel -->
        <div class="bg-yellow-800 text-white p-6 flex-shrink-0 md:w-1/3">
          <h2 class="text-3xl font-bold mb-2 font-papyrus">Bestselling Books Curated for You</h2>
          <p>Discover the top titles loved by our readers. These literary gems are selected for their popularity, value, and emotional resonance.</p>
        </div>
        <!-- Right Panel -->
        <div class="relative bg-gray-800 border border-gray-700 p-6 flex-1 overflow-hidden">
          <div
              ref="productScroll"
              @scroll.passive="onScroll('product')"
              class="flex space-x-6 overflow-x-auto pb-4 scroll-smooth"
          >
            <template v-if="loadingProducts">
              <div v-for="n in 5" :key="n" class="w-60 h-80 bg-gray-700 shadow animate-pulse flex-shrink-0"></div>
            </template>
            <template v-else>
              <BookCard
                  v-for="product in topProducts"
                  :key="product.id"
                  :book="product"
                  class="flex-shrink-0"
              >
                <template #flag>
                  <FlagButton v-if="isAuthenticated" type="book" :id="product.id" />
                </template>
                <template #action-button>
                  <AddToCartButton :product="product" @addToCart="handleAddToCart" />
                </template>
              </BookCard>
            </template>
          </div>
        </div>
      </div>
    </section>

    <!-- Authors Section -->
    <section class="py-16 w-full bg-gray-900">
      <div class="flex flex-col md:flex-row w-full">
        <!-- Left Panel -->
        <div class="bg-yellow-800 text-white p-6 flex-shrink-0 md:w-1/3">
          <h2 class="text-3xl font-bold mb-2 font-papyrus">Meet Our Top Authors</h2>
          <p>These writers have earned praise for their storytelling, insight, and originality. Explore their works and connect with their literary worlds.</p>
        </div>
        <!-- Right Panel -->
        <div class="relative bg-gray-800 border border-gray-700 p-6 flex-1 overflow-hidden">
          <div
              ref="artisanScroll"
              @scroll.passive="onScroll('artisan')"
              class="flex space-x-6 overflow-x-auto pb-4 scroll-smooth"
          >
            <template v-if="loadingArtisans">
              <div v-for="n in 5" :key="n" class="w-60 h-80 bg-gray-700 shadow animate-pulse flex-shrink-0"></div>
            </template>
            <template v-else>
              <AuthorCard
                  v-for="artisan in topArtisans"
                  :key="artisan.id"
                  :author="artisan"
                  class="flex-shrink-0"
              >
                <template #flag>
                  <FlagButton v-if="isAuthenticated" type="author" :id="artisan.id" />
                </template>
              </AuthorCard>
            </template>
          </div>
        </div>
      </div>
    </section>

    <!-- About Section -->
    <section class="py-16 bg-yellow-800 text-center">
      <h2 class="text-3xl sm:text-4xl font-bold text-white mb-6 font-papyrus">About Pustakalaya</h2>
      <p class="text-lg text-yellow-200 mb-8 max-w-2xl mx-auto">
        Pustakalaya is more than a bookstore—it's a celebration of stories. Our platform connects readers with writers who pour passion into every page.
      </p>
      <button
          class="inline-block bg-yellow-700 hover:bg-yellow-800 text-white font-semibold py-3 px-8 rounded-lg shadow-lg transition-transform transform hover:-translate-y-1"
      >
        Meet the Authors
      </button>
    </section>

    <!-- Footer -->
    <Footer />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';
import NavBar from '@/components/NavBar.vue';
import Footer from '@/components/Footer.vue';


import AddToCartButton from '@/components/AddToCartButton.vue';
import FlagButton from '@/components/FlagButton.vue';
import Toast from '@/components/Toast.vue';
import { useCart } from '@/store/useCart';

const { addToCart } = useCart();
const apiBase = process.env.VUE_APP_API_URL;
const isAuthenticated = !!localStorage.getItem('authToken');

const topProducts = ref([]);
const topArtisans = ref([]);
const loadingProducts = ref(true);
const loadingArtisans = ref(true);

const productScroll = ref(null);
const artisanScroll = ref(null);

const toasts = reactive([]);
function removeToast(id) {
  const i = toasts.findIndex(t => t.id === id);
  if (i !== -1) toasts.splice(i, 1);
}
function showToast(msg, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message: msg, type });
  setTimeout(() => removeToast(id), 5000);
}

async function fetchTopProducts() {
  loadingProducts.value = true;
  try {
    const res = await axios.get(`${apiBase}/api/v1/books/top-selling?limit=10`);
    topProducts.value = res.data.data;
  } catch (e) {
    showToast('Failed to load books', 'error');
  } finally {
    loadingProducts.value = false;
  }
}

async function fetchTopArtisans() {
  loadingArtisans.value = true;
  try {
    const res = await axios.get(`${apiBase}/api/v1/authors/top-rated?limit=10`);
    topArtisans.value = res.data.data;
  } catch {
    showToast('Failed to load authors', 'error');
  } finally {
    loadingArtisans.value = false;
  }
}

function onScroll(type) {
  const el = type === 'product' ? productScroll.value : artisanScroll.value;
  if (!el) return;
  const max = el.scrollWidth - el.clientWidth;
}

async function handleAddToCart({ product, quantity }) {
  const res = await addToCart(product, quantity);
  showToast(res.message, res.success ? 'success' : 'error');
}

onMounted(() => {
  fetchTopProducts();
  fetchTopArtisans();
});
</script>

<style scoped>
.bg-beige { background-color: #1a1a1a; }
.font-papyrus { font-family: 'Papyrus', serif; }

@media (max-width: 767px) {
  [ref="productScroll"]::-webkit-scrollbar,
  [ref="artisanScroll"]::-webkit-scrollbar {
    display: none;
  }
}
</style>
