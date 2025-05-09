<template>
  <div class="shop-container min-h-screen flex flex-col bg-gray-900 text-[#C8B280]">
    <NavBar />
    <Toast :toasts="toasts" @close="removeToast" />

    <main class="flex-grow">
      <!-- Filters -->
      <section class="bg-gray-900 py-6 px-4 sm:px-8">
        <div class="container mx-auto space-y-4">
          <!-- Search -->
          <input
              v-model="searchQuery"
              type="text"
              placeholder="Search books..."
              class="w-full p-3 rounded-lg bg-gray-900 text-[#C8B280] placeholder-[#777] border border-[#3a3a3a] focus:outline-none focus:ring-2 focus:ring-[#A0522D]"
          />

          <!-- Filter Row -->
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4 items-start">
            <!-- Genre -->
            <div class="p-3 bg-[#2a2a2a] rounded-lg border border-[#3a3a3a]">
              <label class="block mb-1 text-sm font-semibold">Genre</label>
              <select v-model="selectedGenre" class="w-full p-2 rounded bg-[#121212] text-[#C8B280] border border-[#3a3a3a]">
                <option value="">All Genres</option>
                <option v-for="genre in genres" :key="genre.id" :value="genre.id">
                  {{ genre.name }}
                </option>
              </select>
            </div>

            <!-- Author -->
            <div class="p-3 bg-[#2a2a2a] rounded-lg border border-[#3a3a3a]">
              <label class="block mb-1 text-sm font-semibold">Author</label>
              <select v-model="selectedAuthor" class="w-full p-2 rounded bg-[#121212] text-[#C8B280] border border-[#3a3a3a]">
                <option value="">All Authors</option>
                <option v-for="author in authors" :key="author.id" :value="author.id">
                  {{ author.name }}
                </option>
              </select>
            </div>

            <!-- Publisher -->
            <div class="p-3 bg-[#2a2a2a] rounded-lg border border-[#3a3a3a]">
              <label class="block mb-1 text-sm font-semibold">Publisher</label>
              <select v-model="selectedPublisher" class="w-full p-2 rounded bg-[#121212] text-[#C8B280] border border-[#3a3a3a]">
                <option value="">All Publishers</option>
                <option v-for="publisher in publishers" :key="publisher.id" :value="publisher.id">
                  {{ publisher.name }}
                </option>
              </select>
            </div>

            <!-- Availability -->
            <div class="p-3 bg-[#2a2a2a] rounded-lg border border-[#3a3a3a] space-y-2">
              <span class="block mb-1 text-sm font-semibold">Availability</span>
              <label class="inline-flex items-center">
                <input type="checkbox" v-model="availabilityInStock" class="form-checkbox accent-[#A0522D]" />
                <span class="ml-2 text-sm">In Stock</span>
              </label>
              <label class="inline-flex items-center">
                <input type="checkbox" v-model="availabilityLibrary" class="form-checkbox accent-[#A0522D]" />
                <span class="ml-2 text-sm">Library Access</span>
              </label>
            </div>

            <!-- Rating Range -->
            <div class="p-3 bg-[#2a2a2a] rounded-lg border border-[#3a3a3a]">
              <label class="block mb-2 text-sm font-semibold">Rating Range</label>
              <div class="flex justify-between items-center space-x-4">
                <div class="flex-1">
                  <span class="text-xs">Min: {{ ratingMin.toFixed(1) }}</span>
                  <input
                      type="range"
                      min="0"
                      max="5"
                      step="0.5"
                      v-model.number="ratingMin"
                      @input="validateRating"
                      class="w-full accent-[#A0522D]"
                  />
                </div>
                <div class="flex-1">
                  <span class="text-xs">Max: {{ ratingMax.toFixed(1) }}</span>
                  <input
                      type="range"
                      min="0"
                      max="5"
                      step="0.5"
                      v-model.number="ratingMax"
                      @input="validateRating"
                      class="w-full accent-[#A0522D]"
                  />
                </div>
              </div>
            </div>

            <!-- Price Slider (unchanged) -->
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

            <!-- Sort (unchanged) -->
            <div class="p-3 bg-[#2a2a2a] rounded-lg border border-[#3a3a3a]">
              <label class="block mb-1 text-sm font-semibold">Sort By</label>
              <select v-model="selectedSort" class="w-full p-2 rounded bg-[#121212] text-[#C8B280] border border-[#3a3a3a]">
                <option value="">Default</option>
                <option value="price_desc">Price: High → Low</option>
                <option value="price_asc">Price: Low → High</option>
                <option value="title_asc">Title: A → Z</option>
                <option value="title_desc">Title: Z → A</option>
              </select>
            </div>
          </div>

          <!-- Buttons (unchanged) -->
          <div class="flex space-x-4 mt-2">
            <button @click="fetchBooks" class="bg-[#A0522D] text-white px-4 py-2 rounded hover:bg-[#8B4513] transition">Go</button>
            <button @click="resetFilters" class="bg-[#333] text-[#C8B280] px-4 py-2 rounded hover:bg-[#444] transition">Reset</button>
          </div>
        </div>
      </section>

      <!-- Book Grid & Detail Modal (unchanged) -->
      <section class="py-10 px-4 sm:px-8 bg-gray-900">
        <!-- … -->
      </section>
      <ProductDetail v-if="showProductDetail" :book="selectedBook" @close="closeProductDetail">
        <!-- … -->
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

const { addToCart } = useCart();
const apiBaseUrl = process.env.VUE_APP_API_URL;

// existing filters
const searchQuery = ref('');
const selectedGenre = ref('');
const selectedSort = ref('');
const minPrice = ref(0);
const maxPrice = ref(200);

// ** new filter state **
const selectedAuthor = ref('');
const authors = ref([]);
const selectedPublisher = ref('');
const publishers = ref([]);
const availabilityInStock = ref(false);
const availabilityLibrary = ref(false);
const ratingMin = ref(0);
const ratingMax = ref(5);

const books = ref([]);
const genres = ref([]);
const loading = ref(false);
const isAuthenticated = !!localStorage.getItem('authToken');

// toasts (unchanged)
const toasts = reactive([]);
// … removeToast, showToast, modal handlers …

// existing fetchGenres
async function fetchGenres() {
  try {
    const res = await fetch(`${apiBaseUrl}/api/Book/genres`);
    const data = await res.json();
    genres.value = data.data || data;
  } catch (err) {
    console.error(err);
    showToast('Failed to load genres', 'error');
  }
}

// ** new fetchAuthors & fetchPublishers **
async function fetchAuthors() {
  try {
    const res = await fetch(`${apiBaseUrl}/api/Book/authors`);
    const data = await res.json();
    authors.value = data.data || data;
  } catch (err) {
    console.error(err);
    showToast('Failed to load authors', 'error');
  }
}

async function fetchPublishers() {
  try {
    const res = await fetch(`${apiBaseUrl}/api/Book/publishers`);
    const data = await res.json();
    publishers.value = data.data || data;
  } catch (err) {
    console.error(err);
    showToast('Failed to load publishers', 'error');
  }
}

// existing validatePrice
function validatePrice() {
  if (minPrice.value > maxPrice.value) {
    [minPrice.value, maxPrice.value] = [maxPrice.value, minPrice.value];
  }
}
// ** new validateRating **
function validateRating() {
  if (ratingMin.value > ratingMax.value) {
    [ratingMin.value, ratingMax.value] = [ratingMax.value, ratingMin.value];
  }
}

async function fetchBooks() {
  loading.value = true;
  try {
    const params = new URLSearchParams();
    if (searchQuery.value) params.append('search', searchQuery.value);
    if (selectedGenre.value) params.append('genreId', selectedGenre.value);
    // ** new params **
    if (selectedAuthor.value) params.append('authorId', selectedAuthor.value);
    if (selectedPublisher.value) params.append('publisherId', selectedPublisher.value);
    if (availabilityInStock.value) params.append('inStock', true);
    if (availabilityLibrary.value) params.append('libraryAccess', true);
    params.append('ratingMin', ratingMin.value);
    params.append('ratingMax', ratingMax.value);

    params.append('minPrice', minPrice.value);
    params.append('maxPrice', maxPrice.value);
    if (selectedSort.value) params.append('sort', selectedSort.value);

    const res = await fetch(`${apiBaseUrl}/api/Book/filter?${params}`);
    const raw = await res.json();
    books.value = (raw.data || []).map(book => ({
      ...book,
      category: { name: book.genreName },
      images: book.images?.length ? book.images : ['/images/default-book.jpg'],
    }));
  } catch (err) {
    console.error(err);
    showToast('Failed to load books', 'error');
  } finally {
    loading.value = false;
  }
}

function resetFilters() {
  searchQuery.value = '';
  selectedGenre.value = '';
  selectedAuthor.value = '';
  selectedPublisher.value = '';
  availabilityInStock.value = false;
  availabilityLibrary.value = false;
  ratingMin.value = 0;
  ratingMax.value = 5;
  selectedSort.value = '';
  minPrice.value = 0;
  maxPrice.value = 200;
  fetchBooks();
}

onMounted(() => {
  fetchGenres();
  fetchAuthors();
  fetchPublishers();
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
