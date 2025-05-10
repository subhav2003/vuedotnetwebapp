<template>
  <div class="shop-container min-h-screen flex flex-col bg-[#0f0f0f] text-[#C8B280]">
    <NavBar />
    <Toast :toasts="toasts" @close="removeToast" />

    <main class="flex-grow">
      <!-- Filter Toggle -->
      <section class="bg-[#1a1a1a] px-4 py-4 sm:px-8 border-b border-[#333]">
        <div class="container mx-auto flex justify-between items-center">
          <h2 class="text-xl font-semibold">Shop Books</h2>
          <button @click="showFilters = !showFilters" class="bg-[#A0522D] px-4 py-2 text-white rounded hover:bg-[#8B4513]">
            {{ showFilters ? 'Hide Filters' : 'Show Filters' }}
          </button>
        </div>

        <!-- Filters -->
        <div v-if="showFilters" class="mt-4 container mx-auto space-y-6">
          <!-- Search -->
          <input v-model="searchQuery" type="text" placeholder="Search by title, ISBN or description"
                 class="w-full p-3 rounded-lg bg-[#121212] text-[#C8B280] placeholder-[#777] border border-[#3a3a3a] focus:ring-2 focus:ring-[#A0522D]" />

          <!-- Filter Grid -->
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            <!-- Genre -->
            <div>
              <label class="block mb-1 text-sm font-semibold">Genre</label>
              <select v-model="selectedGenre" class="input-filter">
                <option value="">All</option>
                <option v-for="g in genres" :key="g.id" :value="g.id">{{ g.name }}</option>
              </select>
            </div>

            <!-- Language -->
            <div>
              <label class="block mb-1 text-sm font-semibold">Language</label>
              <select v-model="selectedLanguage" class="input-filter">
                <option value="">All</option>
                <option v-for="l in languages" :key="l" :value="l">{{ l }}</option>
              </select>
            </div>

            <!-- Book Type -->
            <div>
              <label class="block mb-1 text-sm font-semibold">Book Type</label>
              <select v-model="selectedBookType" class="input-filter">
                <option value="">All</option>
                <option v-for="t in bookTypes" :key="t" :value="t">{{ t }}</option>
              </select>
            </div>

            <!-- Publisher -->
            <div>
              <label class="block mb-1 text-sm font-semibold">Publisher</label>
              <select v-model="selectedPublisher" class="input-filter">
                <option value="">All</option>
                <option v-for="p in publishers" :key="p" :value="p">{{ p }}</option>
              </select>
            </div>

            <!-- Price Range Dropdown -->
            <div class="flex flex-col space-y-2">
              <label class="text-sm font-semibold">Min Price</label>
              <select v-model="minPrice" class="input-filter">
                <option v-for="p in priceOptions" :key="p" :value="p">${{ p }}</option>
              </select>

              <label class="text-sm font-semibold">Max Price</label>
              <select v-model="maxPrice" class="input-filter">
                <option v-for="p in priceOptions" :key="p" :value="p">${{ p }}</option>
              </select>
            </div>

            <!-- Rating -->
            <div>
              <label class="block mb-1 text-sm font-semibold">Minimum Rating</label>
              <select v-model="selectedRating" class="input-filter">
                <option value="">Any</option>
                <option v-for="r in [5,4,3,2,1]" :key="r" :value="r">{{ r }} ★ & up</option>
              </select>
            </div>

            <!-- Sort -->
            <div>
              <label class="block mb-1 text-sm font-semibold">Sort By</label>
              <select v-model="selectedSort" class="input-filter">
                <option value="">Default</option>
                <option value="price_desc">Price: High → Low</option>
                <option value="price_asc">Price: Low → High</option>
                <option value="title_asc">Title: A → Z</option>
                <option value="title_desc">Title: Z → A</option>
                <option value="date_desc">Newest First</option>
                <option value="sold_desc">Most Popular</option>
              </select>
            </div>

            <!-- Toggles -->
            <div class="flex flex-col space-y-2">
              <label class="inline-flex items-center space-x-2">
                <input type="checkbox" v-model="exclusiveOnly" class="accent-[#A0522D]" />
                <span>Exclusive Edition</span>
              </label>
              <label class="inline-flex items-center space-x-2">
                <input type="checkbox" v-model="physicalAccessOnly" class="accent-[#A0522D]" />
                <span>Physical Access</span>
              </label>
            </div>
          </div>

          <!-- Buttons -->
          <div class="flex space-x-4 mt-4">
            <button @click="fetchBooks" class="bg-[#A0522D] text-white px-4 py-2 rounded hover:bg-[#8B4513] transition">Apply</button>
            <button @click="resetFilters" class="bg-[#444] text-white px-4 py-2 rounded hover:bg-[#555]">Reset</button>
          </div>
        </div>
      </section>

      <!-- Book Grid -->
      <section class="py-10 px-4 sm:px-8">
        <div class="container mx-auto">
          <div v-if="loading" class="text-center text-[#ccc] py-20">Loading...</div>
          <div v-else-if="books.length === 0" class="text-center text-[#aaa] py-20">No books found.</div>
          <div v-else class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
            <ProductCard
                v-for="book in books"
                :key="book.id"
                :book="book"
                @showBookDetail="openProductDetail"
            >
              <template #action-button>
                <AddToCartButton :product="book" @addToCart="handleAddToCart" />
              </template>
            </ProductCard>
          </div>
        </div>
      </section>

      <!-- Detail Modal -->
      <ProductDetail
          v-if="showProductDetail"
          :book="selectedBook"
          @close="closeProductDetail"
      >
        <template #flag>
          <FlagButton v-if="isAuthenticated" :id="selectedBook.id" type="product" class="ml-4" />
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
import NavBar from '@/components/NavBar.vue';
import Footer from '@/components/Footer.vue';
import ProductCard from '@/components/ProductCard.vue';
import ProductDetail from '@/components/ProductDetail.vue';
import AddToCartButton from '@/components/AddToCartButton.vue';
import FlagButton from '@/components/FlagButton.vue';
import Toast from '@/components/Toast.vue';
import { useCart } from '@/store/useCart';

const apiBaseUrl = process.env.VUE_APP_API_URL;
const { addToCart } = useCart();

// UI + State
const showFilters = ref(false);
const loading = ref(false);
const books = ref([]);
const genres = ref([]);
const publishers = ref([]);
const languages = ref(['English', 'Nepali', 'Hindi']);
const bookTypes = ref(['Hardcover', 'Paperback', 'Signed', 'Limited', 'Collector\'s', 'Deluxe']);
const priceOptions = Array.from({ length: 21 }, (_, i) => i * 5); // $0 to $100 in steps of $5


// Filters
const searchQuery = ref('');
const selectedGenre = ref('');
const selectedBookType = ref('');
const selectedLanguage = ref('');
const selectedPublisher = ref('');
const selectedSort = ref('');
const selectedRating = ref('');
const minPrice = ref(0);
const maxPrice = ref(1000);
const exclusiveOnly = ref(false);
const physicalAccessOnly = ref(false);

// Toast
const toasts = reactive([]);
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 5000);
}
function removeToast(id) {
  const i = toasts.findIndex(t => t.id === id);
  if (i !== -1) toasts.splice(i, 1);
}

// Detail
const showProductDetail = ref(false);
const selectedBook = ref(null);
const isAuthenticated = !!localStorage.getItem('authToken');
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

// Fetchers
async function fetchGenres() {
  const res = await fetch(`${apiBaseUrl}/api/Book/genres`);
  const data = await res.json();
  genres.value = data.data || [];
}
async function fetchBooks() {
  loading.value = true;
  try {
    const params = new URLSearchParams();
    if (searchQuery.value) params.append('search', searchQuery.value);
    if (selectedGenre.value) params.append('genreId', selectedGenre.value);
    if (selectedBookType.value) params.append('bookType', selectedBookType.value);
    if (selectedLanguage.value) params.append('language', selectedLanguage.value);
    if (selectedPublisher.value) params.append('publisher', selectedPublisher.value);
    if (selectedSort.value) params.append('sort', selectedSort.value);
    if (selectedRating.value) params.append('minRating', selectedRating.value);
    if (exclusiveOnly.value) params.append('exclusive', true);
    if (physicalAccessOnly.value) params.append('physical', true);
    params.append('minPrice', minPrice.value);
    params.append('maxPrice', maxPrice.value);

    const res = await fetch(`${apiBaseUrl}/api/Book/filter?${params}`);
    const raw = await res.json();
    books.value = (raw.data || []).map(b => ({
      ...b,
      images: b.images?.length ? b.images : ['/images/default-book.jpg'],
    }));
  } catch (err) {
    showToast('Failed to load books', 'error');
  } finally {
    loading.value = false;
  }
}
function resetFilters() {
  searchQuery.value = '';
  selectedGenre.value = '';
  selectedBookType.value = '';
  selectedLanguage.value = '';
  selectedPublisher.value = '';
  selectedSort.value = '';
  selectedRating.value = '';
  minPrice.value = 0;
  maxPrice.value = 1000;
  exclusiveOnly.value = false;
  physicalAccessOnly.value = false;
  fetchBooks();
}
const handleAddToCart = async ({ product, quantity }) => {
  const res = await addToCart(product, quantity);
  showToast(res.message, res.success ? 'success' : 'error');
};

// Init
onMounted(() => {
  fetchGenres();
  fetchBooks();
});
</script>

<style scoped>
.input-filter {
  width: 100%;
  padding: 0.5rem;
  background-color: #121212;
  border: 1px solid #3a3a3a;
  border-radius: 0.375rem;
  color: #C8B280;
}
.loader {
  border-top-color: #A0522D;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
