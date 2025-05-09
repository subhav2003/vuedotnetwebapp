<template>
  <div class="shop-container min-h-screen flex flex-col bg-[#0f0f0f] text-[#C8B280]">
    <NavBar />
    <Toast :toasts="toasts" @close="removeToast" />

    <main class="flex-grow">
      <!-- Filters -->
      <section class="bg-[#1a1a1a] py-6 px-4 sm:px-8">
        <div class="container mx-auto space-y-4">
          <!-- Search -->
          <input
              v-model="searchQuery"
              type="text"
              placeholder="Search books..."
              class="w-full p-3 rounded-lg bg-[#121212] text-[#C8B280] placeholder-[#777] border border-[#3a3a3a] focus:outline-none focus:ring-2 focus:ring-[#A0522D]"
          />

          <!-- Filter Row -->
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4 items-start">
            <!-- Genre -->
            <div class="p-3 bg-[#2a2a2a] rounded-lg border border-[#3a3a3a]">
              <label class="block mb-1 text-sm font-semibold">Genre</label>
              <select
                  v-model="selectedGenre"
                  class="w-full p-2 rounded bg-[#121212] text-[#C8B280] border border-[#3a3a3a]"
              >
                <option value="">All Genres</option>
                <option v-for="genre in genres" :key="genre.id" :value="genre.id">
                  {{ genre.name }}
                </option>
              </select>
            </div>

            <!-- Price Slider -->
            <div class="p-3 bg-[#2a2a2a] rounded-lg border border-[#3a3a3a]">
              <label class="block mb-2 text-sm font-semibold">Price Range</label>
              <div class="flex justify-between items-center space-x-4">
                <div class="flex-1">
                  <span class="text-xs">Min: ${{ minPrice }}</span>
                  <input
                      type="range"
                      min="0"
                      max="500"
                      step="1"
                      v-model.number="minPrice"
                      @input="validatePrice"
                      class="w-full accent-[#A0522D]"
                  />
                </div>
                <div class="flex-1">
                  <span class="text-xs">Max: ${{ maxPrice }}</span>
                  <input
                      type="range"
                      min="0"
                      max="500"
                      step="1"
                      v-model.number="maxPrice"
                      @input="validatePrice"
                      class="w-full accent-[#A0522D]"
                  />
                </div>
              </div>
            </div>

            <!-- Sort -->
            <div class="p-3 bg-[#2a2a2a] rounded-lg border border-[#3a3a3a]">
              <label class="block mb-1 text-sm font-semibold">Sort By</label>
              <select
                  v-model="selectedSort"
                  class="w-full p-2 rounded bg-[#121212] text-[#C8B280] border border-[#3a3a3a]"
              >
                <option value="">Default</option>
                <option value="price_desc">Price: High → Low</option>
                <option value="price_asc">Price: Low → High</option>
                <option value="title_asc">Title: A → Z</option>
                <option value="title_desc">Title: Z → A</option>
              </select>
            </div>
          </div>

          <!-- Buttons -->
          <div class="flex space-x-4 mt-2">
            <button
                @click="fetchBooks"
                class="bg-[#A0522D] text-white px-4 py-2 rounded hover:bg-[#8B4513] transition"
            >
              Go
            </button>
            <button
                @click="resetFilters"
                class="bg-[#333] text-[#C8B280] px-4 py-2 rounded hover:bg-[#444] transition"
            >
              Reset
            </button>
          </div>
        </div>
      </section>

      <!-- Book Grid -->
      <section class="py-10 px-4 sm:px-8 bg-[#0f0f0f]">
        <div class="container mx-auto">
          <div v-if="loading" class="flex justify-center items-center h-64">
            <div class="loader ease-linear rounded-full border-4 border-t-4 border-[#C8B280] h-12 w-12"></div>
          </div>

          <div v-else-if="books.length === 0" class="text-center text-[#999] mt-20 text-lg">
            No books found.
          </div>

          <div v-else class="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            <ProductCard
                v-for="book in books"
                :key="book.id"
                :book="book"
                @showBookDetail="openProductDetail"
            >
              <template #action-button>
                <!-- Add to Cart -->
                <AddToCartButton :product="book" @addToCart="handleAddToCart" />

                <!-- Wishlist Button -->
                <button
                    @click="addToWishlist(book)"
                    class="ml-2 p-2 bg-[#2a2a2a] rounded hover:bg-[#3a3a3a] transition"
                    title="Add to Wishlist"
                >
                  <font-awesome-icon :icon="['fas','heart']" class="text-lg text-[#C8B280]" />
                </button>
              </template>
            </ProductCard>
          </div>
        </div>
      </section>

      <!-- Detail Modal -->
      <ProductDetail v-if="showProductDetail" :book="selectedBook" @close="closeProductDetail">
        <template #flag>
          <div class="ml-4 mt-1">
            <FlagButton v-if="isAuthenticated" type="product" :id="selectedBook.id" />
          </div>
        </template>
        <template #add-to-cart>
          <AddToCartButton :product="selectedBook" @addToCart="handleAddToCart" />
        </template>
      </ProductDetail>
    </main>

    <Footer />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';

import NavBar from '@/components/NavBar.vue';
import Footer from '@/components/Footer.vue';
import ProductCard from '@/components/ProductCard.vue';
import ProductDetail from '@/components/ProductDetail.vue';
import AddToCartButton from '@/components/AddToCartButton.vue';
import FlagButton from '@/components/FlagButton.vue';
import Toast from '@/components/Toast.vue';
import { useCart } from '@/store/useCart';

const { addToCart } = useCart();
const apiBaseUrl = process.env.VUE_APP_API_URL;
const token = () => localStorage.getItem('authToken');

const searchQuery = ref('');
const selectedGenre = ref('');
const selectedSort = ref('');
const minPrice = ref(0);
const maxPrice = ref(200);
const books = ref([]);
const genres = ref([]);
const loading = ref(false);
const isAuthenticated = !!token();

// Toast stack
const toasts = reactive([]);
function removeToast(id) {
  const i = toasts.findIndex(t => t.id === id);
  if (i !== -1) toasts.splice(i, 1);
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 5000);
}

// Detail modal
const showProductDetail = ref(false);
const selectedBook = ref(null);
function openProductDetail(book) {
  selectedBook.value = book;
  showProductDetail.value = true;
  document.body.style.overflow = 'hidden';
}
function closeProductDetail() {
  selectedBook.value = null;
  showProductDetail.value = false;
  document.body.style.overflow = '';
}

// Fetch genres
async function fetchGenres() {
  try {
    const res = await fetch(`${apiBaseUrl}/api/Book/genres`);
    const data = await res.json();
    genres.value = data.data || data;
  } catch {
    showToast('Failed to load genres', 'error');
  }
}

// Fetch books
async function fetchBooks() {
  loading.value = true;
  try {
    const params = new URLSearchParams();
    if (searchQuery.value) params.append('search', searchQuery.value);
    if (selectedGenre.value) params.append('genreId', selectedGenre.value);
    if (selectedSort.value) params.append('sort', selectedSort.value);
    params.append('minPrice', minPrice.value);
    params.append('maxPrice', maxPrice.value);

    const res = await fetch(`${apiBaseUrl}/api/Book/filter?${params}`);
    const raw = await res.json();
    books.value = (raw.data || []).map(book => ({
      ...book,
      category: { name: book.genreName },
      images: book.images?.length ? book.images : ['/images/default-book.jpg'],
    }));
  } catch {
    showToast('Failed to load books', 'error');
  } finally {
    loading.value = false;
  }
}

// Add to cart
const handleAddToCart = async ({ product, quantity }) => {
  const res = await addToCart(product, quantity);
  showToast(res.message, res.success ? 'success' : 'error');
};

// Add to wishlist (bookmarks)
async function addToWishlist(book) {
  if (!isAuthenticated) {
    showToast('Please log in to add to wishlist', 'error');
    return;
  }
  try {
    const res = await axios.post(
        `${apiBaseUrl}/api/bookmarks/${book.id}`,
        {},
        { headers: { Authorization: `Bearer ${token()}` } }
    );
    showToast(res.data.message || 'Added to wishlist');
  } catch (err) {
    showToast(err.response?.data?.message || 'Failed to add to wishlist', 'error');
  }
}

// Helpers
function resetFilters() {
  searchQuery.value = '';
  selectedGenre.value = '';
  selectedSort.value = '';
  minPrice.value = 0;
  maxPrice.value = 200;
  fetchBooks();
}

function validatePrice() {
  if (minPrice.value > maxPrice.value) {
    [minPrice.value, maxPrice.value] = [maxPrice.value, minPrice.value];
  }
}

onMounted(() => {
  fetchGenres();
  fetchBooks();
});
</script>

<style scoped>
.loader {
  border-top-color: #A0522D;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
